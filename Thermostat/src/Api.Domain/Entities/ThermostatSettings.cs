namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class ThermostatSettings
    {
        public ThermostatSettings(
            string hvacMode,
            int desiredHeat, int desiredCool, 
            int heatRangeHigh, int heatRangeLow, int coolRangeHigh, int coolRangeLow, 
            int heatCoolMinDelta)
        {
            HvacMode = hvacMode;

            DesiredHeat = desiredHeat;
            DesiredCool = desiredCool;

            HeatRangeHigh = heatRangeHigh;
            HeatRangeLow = heatRangeLow;
            CoolRangeHigh = coolRangeHigh;
            CoolRangeLow = coolRangeLow;

            HeatCoolMinDelta = heatCoolMinDelta;
        }

        public string HvacMode { get; }

        public int DesiredHeat { get; }
        public int DesiredCool { get; }

        public int HeatRangeHigh { get; }
        public int HeatRangeLow { get; }

        public int CoolRangeHigh { get; }
        public int CoolRangeLow { get; }

        public int HeatCoolMinDelta { get; }
    }
}