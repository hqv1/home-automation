using System.Collections.Generic;
using MediatR;

namespace Hqv.Thermostat.Api.Models
{
    public class ThermostatToGetModel : ModelBase, IRequest<IEnumerable<ThermostatModel>>
    {
        public bool IncludeReadings { get; set; }
        public bool IncludeSettings { get; set; }
        public bool IncludeScenes { get; set; }
    }
}
