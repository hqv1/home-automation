using System;
using System.Collections.Generic;
using System.Linq;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers
{
    /// <summary>
    /// Parse JSON from Ecobee to a Thermostat
    /// </summary>
    public static class ThermostatListParser
    {
        public static IEnumerable<Domain.Entities.Thermostat> Parse(dynamic json)
        {
            var thermostats = new List<Domain.Entities.Thermostat>();
            var numberOfThermostats = json.thermostatList.Count;
            foreach (int index in Enumerable.Range(0, numberOfThermostats))
            {

                var thermostatJson = json.thermostatList[index];
                var thermostat = new Domain.Entities.Thermostat(
                    id: Convert.ToString(thermostatJson.identifier) ,
                    name: Convert.ToString(thermostatJson.name),
                    brand: Convert.ToString(thermostatJson.brand),
                    model: Convert.ToString(thermostatJson.modelNumber));

                var readingJson = thermostatJson.runtime;
                var readingDateTime = DateTime.Parse((string)readingJson.connectDateTime);
                var temperatureInF = (int) readingJson.actualTemperature;
                var humidity = (int) readingJson.actualHumidity;

                thermostat.Reading = new ThermostatReading(readingDateTime, temperatureInF, humidity);
                thermostats.Add(thermostat);
            }
            return thermostats;
        }
    }
}