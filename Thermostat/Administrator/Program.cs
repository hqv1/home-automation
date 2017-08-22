using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using CommandLine;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Administrator.Actors;
using Hqv.Thermostat.Administrator.Options;
using Microsoft.Extensions.Configuration;

namespace Hqv.Thermostat.Administrator
{
    internal class Program
    {
        private static IConfigurationRoot _config;
        private static IContainer _iocContainer;

        private static int Main(string[] args)
        {
            GetConfigurationRoot();
            _iocContainer = Ioc.RegisterComponents(_config);

            try
            {
                return Parser.Default.ParseArguments<
                        CreateDatabaseOptions>(args)
                    .MapResult(
                        (CreateDatabaseOptions opts) => _iocContainer.Resolve<CreateDatabaseActor>().Act(opts),
                        errs => ProcessError(errs, args)
                    );
            }
            catch (Exception ex)
            {
                var logger = _iocContainer.Resolve<IHqvLogger>();
                logger.Error(ex, "Fatal exception");
                System.Console.WriteLine($"Exception. See logs: {ex.Message}");
                return 1;
            }
        }

        private static void GetConfigurationRoot()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        private static int ProcessError(IEnumerable<Error> errs, string[] args)
        {
            var logger = _iocContainer.Resolve<IHqvLogger>();
            var exception = new Exception("Unable to parse command");
            exception.Data["args"] = string.Join("; ", args) + " --- ";
            exception.Data["errors"] = string.Join("; ", errs.Select(x => x.Tag));
            logger.Error(exception, "Exiting programming");

            return 1;
        }
    }
}