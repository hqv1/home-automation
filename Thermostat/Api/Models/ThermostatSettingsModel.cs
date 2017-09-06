namespace Hqv.Thermostat.Api.Models
{
    public class ThermostatSettingsModel
    {
        public string HvacMode { get; set; }

        public int DesiredHeat { get; set; }
        public int DesiredCool { get; set; }

        public int HeatRangeHigh { get; set; }
        public int HeatRangeLow { get; set; }

        public int CoolRangeHigh { get; set; }
        public int CoolRangeLow { get; set; }

        public int HeatCoolMinDelta { get; set; }
    }
}