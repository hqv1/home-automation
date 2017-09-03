using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Domain
{
    public interface IEventLogger
    {
        Task AddDomainEvent(EventLog eventLog);
        Task AddExceptionDomainEvent(EventLog eventLog);
    }
}