using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram_Bot_LR
{
    public class HandleGPIO
    {
        public GpioController Controller { get; set; }

        public HandleGPIO()
        {

        }

        public bool GetAlarmState()
        {
            return true;
        }

        public bool TogglePin(int pinNumber, PinMode pinMode)
        {
            return false;
        }
    }
}
