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

        // true | false; default false, continously broadcast camera state or on change only
        public static readonly string CONTINOUS = "Continous";


        public static readonly Dictionary<string, string> Switches = new Dictionary<string, string>()
        {
            { "-i", INTERVAL },
            { "-c", CONTINOUS },
            { "-bsl", BLINKSTICK_LEDS_COUNT },
            { "-surl", SIGNALR_URL },
            { "--interval", INTERVAL },
            { "--continous", CONTINOUS },
            { "--blinkstickleds", BLINKSTICK_LEDS_COUNT },
            { "--signalrurl", SIGNALR_URL }
        };
    }    
}