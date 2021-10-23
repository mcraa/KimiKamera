using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlinkStickDotNet;
using Microsoft.Extensions.Logging;

public class BlinksticService : IStatusNotifier
{
    private BlinkStick[] _squareDevices;
    //private UsbMonitor _usbMonitor;
    public Boolean HasDevices { get { return _squareDevices != null && _squareDevices.Length > 0; }}
    private List<byte> _ledIndexes = new List<byte>();

    public BlinksticService(byte ledCount, ILogger _logger) 
    {
         for (byte i = 0; i < ledCount; ++i) 
        {
            _ledIndexes.Add(i);
        }

        // TODO: this gets LibUsbDotnet incorrect format / HidSharp cant load Platform error
        // _usbMonitor = new UsbMonitor();
        // _usbMonitor.BlinkStickConnected += (object sender, DeviceModifiedArgs e) => {
        //     FindDevices();
        // };
        // _usbMonitor.BlinkStickDisconnected += (object sender, DeviceModifiedArgs e) => {
        //     _squareDevices = null;
        // };
    }

    private void SetLedsToColor(string color) {
        foreach (var led in _squareDevices)
        {
            if (led.OpenDevice()) 
            {
                foreach (var i in _ledIndexes)
                {
                    led.Blink(0, i, i%2==0?color:"blue");
                    led.Morph(0, i, color);
                }
            }
            led.CloseDevice();
        }
    }

    private void SetLedsToColor(RgbColor color) {
        foreach (var led in _squareDevices)
        {
            if (led.OpenDevice()) 
            {
                foreach (var i in _ledIndexes)
                {
                    led.Blink(0, i, i%2==0?color:RgbColor.FromString("blue"));
                    led.SetColor(0, i, color);
                }
            }
            led.CloseDevice();
        }
    }

    private void TurnOffLeds() {
        foreach (var led in _squareDevices)
        {
            SetLedsToColor(RgbColor.Black());
            led.TurnOff();
            led.CloseDevice();           
        }
    }

    public BlinksticService FindDevices() {
        _squareDevices = BlinkStick.FindAll();
        return this;
    }

   
    public void Dispose()
    {
        TurnOffLeds();
    }

    public async Task SetBusyAsync()
    {
        SetLedsToColor("red");
    }

    public async Task SetInTransitionAsync()
    {
        SetLedsToColor("orange");
    }

    public async Task SetAvailableAsync()
    {
        SetLedsToColor("green");
    }

    public async Task<bool> IsConnected()
    {
        return HasDevices;
    }
}