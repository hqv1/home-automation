using System.Collections.Generic;
using Hqv.CSharp.Common.Entities;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    /// <summary>
    /// todo: to be a proper DDD entity, we need to add methods to add Reading, Settings, and Scenes and remove the sets
    /// </summary>
    public class Thermostat : IAggregateRoot
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