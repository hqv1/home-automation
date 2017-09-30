using System;

namespace Hqv.Thermostat.Api.Models
{
    public class ThermostatReadingModel
    {
        public DateTime DateTime { get; set; }
        public int TemperatureInF { get; set; }
        public int Humidity { get; set; }
    }
}