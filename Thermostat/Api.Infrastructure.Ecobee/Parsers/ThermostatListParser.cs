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
                Domain.Entities.Thermostat thermostat = CreateThermostat(thermostatJson);
                
                thermostat.Reading = CreateReading(thermostatJson);
                thermostat.Settings = CreateSettings(thermostatJson);
                thermostat.Scenes = CreateScenes(thermostatJson);
                thermostats.Add(thermostat);
            }
            return thermostats;
        }

        private static Domain.Entities.Thermostat CreateThermostat(dynamic thermostatJson)
        {
            return new Domain.Entities.Thermostat(
                id: Convert.ToString(thermostatJson.identifier) ,
                name: Convert.ToString(thermostatJson.name),
                brand: Convert.ToString(thermostatJson.brand),
                model: Convert.ToString(thermostatJson.modelNumber));
        }

        private static ThermostatReading CreateReading(dynamic thermostatJson)
        {
            var readingJson = thermostatJson.runtime;
            var readingDateTime = DateTime.Parse((string)readingJson.connectDateTime);
            var temperatureInF = (int)readingJson.actualTemperature;
            var humidity = (int)readingJson.actualHumidity;

            return new ThermostatReading(readingDateTime, temperatureInF, humidity);
        }

        private static ThermostatSettings CreateSettings(dynamic thermostatJson)
        {
            var settingJson = thermostatJson.settings;
            var hvacMode = (string) settingJson.hvacMode;
            var heatRangeHigh = (int) settingJson.heatRangeHigh;
            var heatRangeLow = (int) settingJson.heatRangeLow;
            var coolRangeHigh = (int) settingJson.coolRangeHigh;
            var coolRangeLow = (int) settingJson.coolRangeLow;
            var heatCoolMinDelta = (int) settingJson.heatCoolMinDelta;

            var readingJson = thermostatJson.runtime;
            var desiredHeat = (int) readingJson.desiredHeat;
            var desiredCool = (int) readingJson.desiredCool;

            return new ThermostatSettings(hvacMode, 
                desiredHeat, desiredCool,
                heatRangeHigh, heatRangeLow, coolRangeHigh, coolRangeLow,
                heatCoolMinDelta);
        }

        private static IEnumerable<ThermostatScene> CreateScenes(dynamic thermostatJson)
        {
            var scenes = new List<ThermostatScene>();
            var eventCount = thermostatJson.events.Count;
            foreach (int index in Enumerable.Range(0, eventCount))
            {
                var eventJson = thermostatJson.events[index];
                scenes.Add(CreateScene(eventJson));
            }
            return scenes;
        }

        private static ThermostatScene CreateScene(dynamic eventJson)
        {
            var type = (string) eventJson.type;
            var name = (string) eventJson.name;
            var running = Convert.ToBoolean( (string) eventJson.running);
            var coolHoldTemp = (int) eventJson.coolHoldTemp;
            var heatHoldTemp = (int) eventJson.heatHoldTemp;

            return new ThermostatScene(type, name, running, coolHoldTemp, heatHoldTemp);
        }
    }
}