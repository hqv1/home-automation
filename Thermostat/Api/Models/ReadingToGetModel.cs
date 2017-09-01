using MediatR;

namespace Hqv.Thermostat.Api.Models
{
    public class ReadingToGetModel : ModelBase, IRequest<object>
    {
        public bool IncludeReadings { get; set; }
        public bool IncludeSettings { get; set; }
        public bool IncludeScenes { get; set; }
    }
}
