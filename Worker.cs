
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace KimiKamera
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly int _interval;
        private Boolean _cameraIsOn = true ;
        private Boolean _cameraIsChanging = true;
        private string _command;        
        private Dictionary<string, IStatusNotifier> _services = new Dictionary<string, IStatusNotifier>();

        public Worker(
            ILogger<Worker> logger,
            IConfiguration args,
            string commandOverride = @"wmic process where ""CommandLine like '%-k Camera%' and not CommandLine like '%wmic%'"" get ThreadCount"
        )
        {
            _logger = logger;
            _interval = string.IsNullOrEmpty(args[ArgNames.INTERVAL]) ? 5000 : Int16.Parse(args[ArgNames.INTERVAL]);
            
            ParseBLinkStickParams(args[ArgNames.LEDS_COUNT]);
            ParseSignalRParams(args[ArgNames.SIGNALR_URL]);
            
            _command = commandOverride; 

            // TODO: this gets LibUsbDotnet incorrect format error
            // _usbMonitor = new UsbMonitor();
            // _usbMonitor.BlinkStickConnected += (object sender, DeviceModifiedArgs e) => {
            //     FindDevices();
            // };
            // _usbMonitor.BlinkStickDisconnected += (object sender, DeviceModifiedArgs e) => {
            //     _squareDevices = null;
            // };

        }

        private void ParseSignalRParams(string arg)
        {
            if (!string.IsNullOrEmpty(arg)) 
            {
                _services.Add(nameof(SignalRService), new SignalRService(arg, _logger).Connect());
            }
        }

        private void ParseBLinkStickParams(string arg)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                byte ledCount = Byte.Parse(arg);
                _services.Add(nameof(BlinksticService), new BlinksticService(ledCount, _logger).FindDevices());                           
            }
        }

        public async Task SendStatuses(StatusEnum status)       
        {
            foreach (var srv in _services)
            {
                switch (status)
                {
                    case StatusEnum.Busy:
                    await srv.Value.SetBusyAsync();
                    break;
                    case StatusEnum.Transition:
                    await srv.Value.SetInTransitionAsync();
                    break;
                    case StatusEnum.Available:
                    await srv.Value.SetAvailableAsync();
                    break;
                }
            }
        } 

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
                    var cmd = new Process();
                    cmd.StartInfo = new ProcessStartInfo("cmd", $"/c {_command}");
                    cmd.StartInfo.RedirectStandardOutput = true;

                    cmd.Start();

                    var result = await cmd.StandardOutput.ReadToEndAsync();
                    int count = 0;
                    _logger.LogInformation(result);

                    if (result.Length > 0 && result[0] == 'T')
                    {
                        // Header line should be 'TreadCount'
                        // Then the value in the first line
                        count = Int16.Parse(result.Split('\n')[1].Trim());

                        if (count > 10)
                        {
                            if (!_cameraIsOn) {
                                _logger.LogInformation("Turning camera ON");
                                await SendStatuses(StatusEnum.Busy);

                                _cameraIsOn = true;
                                _cameraIsChanging = false;
                            } 
                        }
                        else
                        {   
                            if (!_cameraIsChanging) {
                                _logger.LogInformation("Cam changing");
                                await SendStatuses(StatusEnum.Transition);
                                
                                _cameraIsChanging = true;
                                _cameraIsOn = false;
                            }
                        }
                    }
                    else
                    {
                        if (_cameraIsOn || _cameraIsChanging) {
                            _logger.LogInformation("Turning camera OFF");
                            await SendStatuses(StatusEnum.Available);
                            
                            _cameraIsChanging = false;
                            _cameraIsOn = false;
                        }
                    }

                    await cmd.WaitForExitAsync();
                    cmd.Dispose();

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