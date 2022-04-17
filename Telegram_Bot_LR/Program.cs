using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using Telegram_Bot_LR.models;
using Telegram_Bot_LR;

var _configPath = Path.Combine(Environment.CurrentDirectory, "appconfig.json");
var _appConfig = new TelegramBotAppConfig();

TelegramBotClient botClient = new TelegramBotClient(string.Empty);

if (System.IO.File.Exists(_configPath))
{
    var fileContents = System.IO.File.ReadAllText(_configPath);
    if (!string.IsNullOrEmpty(fileContents))
    {
        _appConfig = JsonConvert.DeserializeObject<TelegramBotAppConfig>(fileContents);
        botClient = new TelegramBotClient(_appConfig.TelegramAccessToken);
    }
}
else
{
    throw new Exception("Could not read application config");
}

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } // receive all update types
};
botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Type != UpdateType.Message)
        return;
    // Only process text messages
    if (update.Message!.Type != MessageType.Text)
        return;

    var chatId = update.Message.Chat.Id;
    var messageText = update.Message.Text;

    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    string reply = HandleMessage(messageText);
    // Echo received message text
    Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: reply,
        cancellationToken: cancellationToken);
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

string HandleMessage(string incoming)
{
    var options = GetHelpOptions();
    var result = string.Empty;

    if (options != null && options.Any())
    {
        foreach (var option in options)
        {
            result += $"{option}{System.Environment.NewLine}";
        }
    }

    if (incoming.ToLower() == "help")
    {
        return result;
    }
    else
    {
        result = HandleInstruction(incoming);
    }

    if (string.IsNullOrEmpty(result))
    {
        result = "Hello";

    }

    return result;
}

string HandleInstruction(string incoming)
{
    var raspi = new HandleGPIO();
    var response = string.Empty;
    if (incoming == "1")
    {
        var togglestate = raspi.TogglePin(18, System.Device.Gpio.PinMode.Input);
        response = togglestate.ToString();
    }

    return response;
}

List<string> GetHelpOptions()
{
    var response = new List<string>() {
        "1 - Toggle Gate",
        "2 - Toggle Generator",
        "3 - Get Temp",
        "4 - Toggle Lights",
        "5 - Read Inverter Voltage",
        "6 - Set Alarm",
        "7 - Send Image"
    };
    return response;
}