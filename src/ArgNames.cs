using System.Collections.Generic;

namespace KimiKamera
{
    public struct ArgNames
    {
        // the interval of checking for busy camera
        public static readonly string INTERVAL = "Interval";

        // count of blinkstick leds to use
        public static readonly string BLINKSTICK_LEDS_COUNT = "BlinkstickLedsCount";

        // url of signalR hub to notify
        public static readonly string SIGNALR_URL = "SignalRurl";


        public static readonly Dictionary<string, string> Switches = new Dictionary<string, string>()
        {
            { "-i", INTERVAL },
            { "-bsl", BLINKSTICK_LEDS_COUNT },
            { "-surl", SIGNALR_URL },
            { "--interval", INTERVAL },
            { "--blinkstickleds", BLINKSTICK_LEDS_COUNT },
            { "--signalrurl", SIGNALR_URL }
        };
    }    
}