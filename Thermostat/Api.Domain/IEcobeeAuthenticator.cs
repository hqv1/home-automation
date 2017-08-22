using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Domain
{
    public interface IEcobeeAuthenticator
    {
        Task GetBearerToken(Client client);
    }
}