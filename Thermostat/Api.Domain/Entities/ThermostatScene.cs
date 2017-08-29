namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class ThermostatScene
    {
        public ThermostatScene(string type, string name, bool running, int coolHoldTemp, int heatHoldTemp)
        {
            Type = type;
            Name = name;
            Running = running;
            CoolHoldTemp = coolHoldTemp;
            HeatHoldTemp = heatHoldTemp;
        }

        public string Type { get; }
        public string Name { get; }
        public bool Running { get; }
        public int CoolHoldTemp { get; }
        public int HeatHoldTemp { get; }
    }
}