using MediatR;

namespace Hqv.Thermostat.Api.Models
{
    public class SceneToAddModel : ModelBase, IRequest<object>
    {
        public int HeatHoldTemp { get; set; }
        public int ColdHoldTemp { get; set; }
    }
}