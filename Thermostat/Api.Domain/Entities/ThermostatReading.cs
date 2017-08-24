using System;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class ThermostatReading
    {
        public ThermostatReading(DateTime dateTime, int temperatureInF, int humidity)
        {
            DateTime = dateTime;
            TemperatureInF = temperatureInF;
            Humidity = humidity;
        }

        public DateTime DateTime { get; }
        public int TemperatureInF { get; }
        public int Humidity { get; }      
    }
}