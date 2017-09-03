using System.Collections.Generic;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Dtos;

namespace Hqv.Thermostat.Api.Domain
{
    public interface IThermostatProvider
    {
        Task<IEnumerable<Entities.Thermostat>> GetThermostats(GetThermostatsRequest request);
    }
}