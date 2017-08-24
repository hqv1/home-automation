using System.Threading.Tasks;
using MediatR;

namespace Hqv.Thermostat.Api.Messages
{
    public class GetThermostatReadingMessage : IRequest<object>
    {
    }
}
