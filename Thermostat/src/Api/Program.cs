using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Hqv.Thermostat.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Error()
                .WriteTo.File("log/global_logs.json")
                .CreateLogger();

            try
            {
                var host = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .UseApplicationInsights()
                    .Build();

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Error on building and running host");
                throw;
            }
        }
    }
}
