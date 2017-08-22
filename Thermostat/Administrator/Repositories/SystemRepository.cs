using System.IO;
using System.Threading.Tasks;
using Hqv.CSharp.Common.App;
using Hqv.CSharp.Common.Exceptions;

namespace Hqv.Thermostat.Administrator.Repositories
{
    public class SystemRepository : ISystemRepository
    {
        private readonly Settings _settings;

        public class Settings
        {
            public Settings(string sqlCmdPath, string connectionString)
            {
                SqlCmdPath = sqlCmdPath;
                ConnectionString = connectionString;
            }

            public string SqlCmdPath { get; }
            public string ConnectionString { get; }
        }

        public SystemRepository(Settings settings)
        {
            _settings = settings;
        }

        public Task CreateDatabase()
        {
            const string scriptPath = @"Scripts/hqv-thermostat-create-database.sql";

            if (!File.Exists(scriptPath))
            {  
                throw new HqvException($"Script to create database does not exist at {scriptPath}");
            }
           
            var arguments = $"{_settings.ConnectionString} -i {scriptPath}";
            var app = new CommandLineApplication();
            var commandLineResult = app.Run(_settings.SqlCmdPath, arguments);
            if (string.IsNullOrEmpty(commandLineResult.ErrorData)) return Task.CompletedTask;

            var exception = new HqvException($"Error running script");
            exception.Data["path"] = _settings.SqlCmdPath;
            exception.Data["arguments"] = arguments;
            exception.Data["error-message"] = commandLineResult.ErrorData;
            throw exception;
        }        
    }
}