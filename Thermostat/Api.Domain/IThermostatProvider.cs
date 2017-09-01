using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hqv.Thermostat.Api.Domain
{
    public interface IThermostatProvider
    {
        Task<IEnumerable<Entities.Thermostat>> GetThermostats(GetThermostatsRequest request);
    }
}