using System;
using Autofac;
using Hqv.CSharp.Common.Audit;
using Hqv.CSharp.Common.Audit.Logging;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Administrator.Actors;
using Hqv.Thermostat.Administrator.Repositories;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Hqv.Thermostat.Administrator
{
    /// <summary>
    /// IOC container using Autofac (https://autofac.org/)
    /// </summary>
    internal static class Ioc
    {
        public static IContainer RegisterComponents(IConfigurationRoot config)
        {            
            var builder = new ContainerBuilder();
            builder.RegisterInstance(config).As<IConfiguration>();

            RegisterLogging(builder, config);

            builder.RegisterType<AuditorResponseBase>().As<IAuditor>();
            builder.RegisterInstance(new AuditorResponseBase.Settings(
                Convert.ToBoolean(config["auditing:audit-on-successful-event"]),
                Convert.ToBoolean(config["auditing:detail-audit-on-successful-event"])));

            builder.RegisterType<SystemRepository>().As<ISystemRepository>();
            builder.RegisterInstance(new SystemRepository.Settings(
                config["sqlcmd:path"], config["sqlcmd:connection-string"]));            

            builder.RegisterType<CreateDatabaseActor>();

            return builder.Build();
        }

        private static void RegisterLogging(ContainerBuilder builder, IConfiguration config)
        {
            try
            {
                var loggingPath = config["logging:path"];
                if (string.IsNullOrEmpty(loggingPath))
                {
                    const string message = "logging:path cannot be empty in configuration file";
                    System.Console.WriteLine(message);
                    throw new Exception();
                }

                if (!Enum.TryParse(config["logging:minimum-level"], out LogEventLevel level))
                {
                    const string message = "logging:minimum-level is incorrect in configuration file";
                    System.Console.WriteLine(message);
                    throw new Exception();
                }
                var logLevelSwitch = new LoggingLevelSwitch { MinimumLevel = level };

                builder.RegisterType<CSharp.Common.Logging.Serilog.Logger>().As<IHqvLogger>();
                builder.RegisterInstance(new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(logLevelSwitch)
                    .WriteTo.File(new JsonFormatter(), loggingPath)
                    .CreateLogger()).As<ILogger>();
            }
            catch (Exception)
            {
                Console.WriteLine("Logging registration failed");
                throw;
            }
        }
    }
}