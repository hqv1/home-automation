namespace Hqv.Thermostat.Api.Models
{
    public class ThermostatSceneModel
    {       
        public string Type { get; set; }
        public string Name { get; set; }
        public bool Running { get; set; }
        public int CoolHoldTemp { get; set; }
        public int HeatHoldTemp { get; set; }
    }
}