using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Domain.Repositories
{
    public interface IEventLogRepository
    {
        Task Add(EventLog eventLog);
    }
}