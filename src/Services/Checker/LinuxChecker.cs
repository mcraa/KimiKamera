using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class LinuxChecker : IOSCameraChecker {
    private string _command;
    private readonly ILogger _logger;

    public LinuxChecker(ILogger logger) 
    {
        _logger = logger;
    }

    private async Task<int> CheckWithLsmod()
    {
        var result = 0;
        var cmd = new Process();
        cmd.StartInfo = new ProcessStartInfo("bash", "lsmod | grep uvcvideo |  awk 'FNR <= 1' | awk '{{print $3}}'");

        cmd.StartInfo.RedirectStandardOutput = true;   

        cmd.Start();

        // should be 0, 1 or similar or nothing
        var output = await cmd.StandardOutput.ReadToEndAsync();
        await cmd.WaitForExitAsync();
        cmd.Dispose();

        if (!string.IsNullOrEmpty(output)) 
        {
            if (!Int32.TryParse(output, out result))
            {
                _logger.LogError("Can't parse lsof output:", output);
            }
        }

        return result;
    }

    private async Task<List<string>> ListVideoDevices()
    {
        var result = new List<string>();
        var cmd = new Process();
        cmd.StartInfo = new ProcessStartInfo("bash", "ls /dev/ | grep video");

        cmd.StartInfo.RedirectStandardOutput = true;   

        cmd.Start();

        // should /dev/video0 \n /dev/video1 ...
        var output = await cmd.StandardOutput.ReadToEndAsync();
        await cmd.WaitForExitAsync();
        cmd.Dispose();

        if (!string.IsNullOrEmpty(output))
        {
            result = output.Split('\n').ToList();
        }

        return result;
    }

    private async Task<bool> CheckWithLsof(string devicePath)
    {
        var cmd = new Process();
        cmd.StartInfo = new ProcessStartInfo("bash", $"lsof /dev/{devicePath}");

        cmd.StartInfo.RedirectStandardOutput = true;   

        cmd.Start();

        // should be empty if no usage
        var output = await cmd.StandardOutput.ReadToEndAsync();
        await cmd.WaitForExitAsync();
        cmd.Dispose();

        if (!string.IsNullOrEmpty(output)) 
        {
            // not empty -> occupied
            return true;
        }

        // empty -> available
        return false;
    }

    public async Task<StatusEnum> PollCamerasAsync()
    {
        var retVal = StatusEnum.Transition;

        var devices = await ListVideoDevices();
        var occupied_by_lsof = await devices.AnyAsync(async (pathend) => await CheckWithLsof(pathend));
        var occupied_by_lsmod = await CheckWithLsmod();

        if (!occupied_by_lsof && occupied_by_lsmod == 0) 
        {
            retVal = StatusEnum.Available;
        }
        else
        {
            retVal = StatusEnum.Busy;
        }

        return retVal;    
    }
}