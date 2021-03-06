using System.Data;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Infrastructure.Data
{
    public interface IEventLogDatabaseRepository
    {
        Task Add(EventLog eventLog, IDbConnection connection);
    }
}