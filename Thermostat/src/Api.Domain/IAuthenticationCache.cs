using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Domain
{
    public interface IAuthenticationCache
    {
        Client GetToken();
        void SetToken(Client client);
    }
}