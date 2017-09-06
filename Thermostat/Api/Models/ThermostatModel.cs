using System.Collections.Generic;

namespace Hqv.Thermostat.Api.Models
{
    public class ThermostatModel
    {
        public ThermostatModel()
        {
            Scenes = new List<ThermostatSceneModel>();
        }

        public string CorrelationId { get; set; }
        public string Name { get; set; }

        public ThermostatReadingModel Reading { get; set; }
        public ThermostatSettingsModel Settings { get; set; }
        public IEnumerable<ThermostatSceneModel> Scenes { get; set; }
    }    
}