namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class Scene
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