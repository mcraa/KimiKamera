using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class WindowsChecker : IOSCameraChecker {
    private string _command;
    private readonly ILogger _logger;

    public WindowsChecker(ILogger logger, string commandOverride = @"wmic process where ""CommandLine like '%-k Camera%' and not CommandLine like '%wmic%'"" get ThreadCount") 
    {
        _command = commandOverride;
        _logger = logger;
    }

    public async Task<StatusEnum> PollCamerasAsync()
    {
        Console.WriteLine(_command);
        var retVal = StatusEnum.Transition;
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
                retVal = StatusEnum.Busy;                
            }
            else
            {   
                retVal = StatusEnum.Transition;                
            }
        }
        else
        {
            retVal = StatusEnum.Available;            
        }

        await cmd.WaitForExitAsync();
        cmd.Dispose(); 

        return retVal;    
    }
}