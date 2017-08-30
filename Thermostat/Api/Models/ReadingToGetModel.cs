using MediatR;

namespace Hqv.Thermostat.Api.Models
{
    public class ReadingToGetModel : ModelBase, IRequest<object>
    {
    }
}
