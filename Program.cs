using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace KimiKamera
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) 
        {        
            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(chost => {
                    chost.AddCommandLine(args, ArgNames.Switches);
                })
                .ConfigureAppConfiguration((hostC, cApp) => {
                    cApp.AddCommandLine(args, ArgNames.Switches);
                })
                .UseWindowsService(options =>
                {
                    options.ServiceName = "OnAir Cam to BlinkStick";
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // TODO pass parameters to change interval and query
                    services.AddHostedService<Worker>();
                });
    }
    }
}
