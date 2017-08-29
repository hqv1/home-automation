using System.Collections.Generic;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class Thermostat
    {
        public Thermostat(string id, string name, string brand, string model)
        {
            Id = id;
            Name = name;
            Brand = brand;
            Model = model;           
        }

        public string Id { get; }
        public string Name { get; }
        public string Brand { get; }
        public string Model { get; }

        public ThermostatReading Reading { get; set; }

        public ThermostatSettings Settings { get; set; }

        public IEnumerable<ThermostatScene> Scenes { get; set; }
    }
}