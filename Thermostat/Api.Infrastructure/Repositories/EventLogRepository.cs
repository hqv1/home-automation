using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;

namespace Hqv.Thermostat.Api.Infrastructure.Repositories
{
    public class EventLogRepository : IEventLogRepository, IEventLogDatabaseRepository
    {
        public class Settings
        {
            public Settings(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public string ConnectionString { get; }
        }

        private readonly Settings _settings;

        public EventLogRepository(Settings settings)
        {
            _settings = settings;
        }

        public async Task Add(EventLog eventLog)
        {
            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                await Add(eventLog, connection);
            }
        }

        public async Task Add(EventLog eventLog, IDbConnection connection)
        {
            var model = Models.EventLogModel.ConvertFrom(eventLog);
            await connection.InsertAsync(model);
        }
    }
}