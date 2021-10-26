
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace KimiKamera
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly int _interval;
        private readonly Boolean _continous = false;
        private Boolean _cameraIsOn = true ;
        private Boolean _cameraIsChanging = true;
        private string _command;        
        private ServicesCollection _services = new ServicesCollection();
        private IOSCameraChecker _checker;

        public Worker(
            ILogger<Worker> logger,
            IConfiguration args,
            string commandOverride = null
        )
        {
            _logger = logger;
            _interval = ParseIntervalParam(args[ArgNames.INTERVAL]);
            _continous = ParseContinousParam(args[ArgNames.CONTINOUS]);
            ParseBLinkStickParam(args[ArgNames.BLINKSTICK_LEDS_COUNT]);
            ParseSignalRParam(args[ArgNames.SIGNALR_URL]);
            
            _command = commandOverride;

            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isOsx = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            if (isWindows) 
            {
                _checker = 
                    string.IsNullOrEmpty(commandOverride) 
                    ? new WindowsChecker(_logger) 
                    : new WindowsChecker(_logger, _command);
            } 
            else if (isLinux) 
            {
                Console.WriteLine("linx");
            }
            else if (isOsx)
            {
                Console.WriteLine("osx");
            }
            else 
            {
                _logger.LogError("Unsupported platform", RuntimeInformation.RuntimeIdentifier);
                throw new Exception($"Unsupported platform {RuntimeInformation.RuntimeIdentifier}");
            }
        }

        #region Params 

        private int ParseIntervalParam(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return 5000;
            } 
            else
            {
                return Int16.Parse(arg);
            }
        }

        private void ParseSignalRParam(string arg)
        {
            if (!string.IsNullOrEmpty(arg)) 
            {
                _services.Add(nameof(SignalRService), new SignalRService(arg, _logger).Connect());
            }
        }

        private void ParseBLinkStickParam(string arg)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                byte ledCount = Byte.Parse(arg);
                _services.Add(nameof(BlinksticService), new BlinksticService(ledCount, _logger).FindDevices());                           
            }
        }

        private bool ParseContinousParam(string arg) 
        {
            if (!string.IsNullOrEmpty(arg) && string.Equals("true", arg, StringComparison.InvariantCultureIgnoreCase)) 
            {
                return true;
            }

            return false;
        }

        #endregion


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            while (!stoppingToken.IsCancellationRequested)
            {
                // workaround for UsbMonitor support
                if (_services.TryGetValue(nameof(BlinksticService), out IStatusNotifier bs))
                {
                    (bs as BlinksticService).FindDevices();
                }   

                // if no services available do not run the queries
                if (_services.Count < 1 || _services.All(s => !s.Value.IsConnected().Result)) {
                    await Task.Delay(_interval, stoppingToken);
                    continue;
                }

                try
                {                    
                    var current = await _checker.PollCamerasAsync();

                    // sending status on changing
                    if (current != StatusEnum.Available)
                    {
                        if (current == StatusEnum.Busy)
                        {
                            if (!_cameraIsOn) {
                                _logger.LogInformation("Turning camera ON");
                                await _services.SendStatuses(current);

                                _cameraIsOn = true;
                                _cameraIsChanging = false;
                            } 
                        }
                        else
                        {   
                            if (!_cameraIsChanging) {
                                _logger.LogInformation("Cam changing");
                                await _services.SendStatuses(current);
                                
                                _cameraIsChanging = true;
                                _cameraIsOn = false;
                            }
                        }
                    }
                    else
                    {
                        if (_cameraIsOn || _cameraIsChanging) {
                            _logger.LogInformation("Turning camera OFF");
                            await _services.SendStatuses(current);
                            
                            _cameraIsChanging = false;
                            _cameraIsOn = false;
                        }
                    }

                    // sending status every interval
                    if (_continous) await _services.SendStatuses(current);

                }
                catch (System.Exception e)
                {
                    _logger.LogError($"[onair-cam]::[Error] :: {e} | {e.Message}");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        public override void Dispose()
        {
            foreach(var srv in _services)
            {
                srv.Value.Dispose();
            }

            base.Dispose();
        }


    }
}