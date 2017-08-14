using System;
using Hqv.CSharp.Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Hqv.Thermostat.Api
{
    public static class Ioc
    {
        public static void Register(IServiceCollection services, IConfigurationRoot configuration)
        {
            RegisterLogging(services, configuration);
        }

        private static void RegisterLogging(IServiceCollection services, IConfiguration configuration)
        {
            var loggingPath = configuration["logging:path"];
            if (string.IsNullOrEmpty(loggingPath))
            {
                const string message = "logging:path cannot be empty in configuration";
                Log.Logger.Fatal(message);
                throw new Exception(message);
            }

            if (!Enum.TryParse(configuration["logging:minimum-level"], out LogEventLevel level))
            {
                const string message = "logging:minimum-level is incorrect in configuration file";
                Log.Logger.Fatal(message);
                throw new Exception(message);
            }
            var logLevelSwitch = new LoggingLevelSwitch {MinimumLevel = level};

            services.AddSingleton<IHqvLogger, CSharp.Common.Logging.Serilog.Logger>();
            services.AddSingleton<ILogger>(
                new LoggerConfiguration().MinimumLevel.ControlledBy(logLevelSwitch)
                    .WriteTo.File(new JsonFormatter(), loggingPath)
                    .CreateLogger()
            );
        }
    }
}
