using Hqv.CSharp.Common.Entities;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    /// <summary>
    /// todo: Scene should only be added through the Thermostat (DDD design). Also there's some logic on what the temp can be depending on state of Thermostat.
    /// </summary>
    public class Scene : IAggregateRoot
    {
        public Scene(int heatHoldTemp, int coolHoldTemp)
        {
            HeatHoldTemp = heatHoldTemp;
            CoolHoldTemp = coolHoldTemp;

        }
        public int HeatHoldTemp { get; }
        public int CoolHoldTemp { get; }
    }
}