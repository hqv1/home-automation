using MediatR;

namespace Hqv.Thermostat.Api.Models
{
    public class ReadingToGet : ModelBase, IRequest<object>
    {
    }
}
