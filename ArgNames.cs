using System.Collections.Generic;

namespace KimiKamera
{
    public struct ArgNames
    {
        // the interval of checking for busy camera
        public static readonly string INTERVAL = "interval";

        // count of blinkstick leds to use
        public static readonly string LEDS_COUNT = "blinkstickleds";

        // url of signalR hub to notify
        public static readonly string SIGNALR_URL = "signalRurl";


        public static readonly Dictionary<string, string> Switches = new Dictionary<string, string>()
        {
            { "-i", INTERVAL },
            { "-bsl", LEDS_COUNT },
            { "-surl", SIGNALR_URL }
        };
    }    
}