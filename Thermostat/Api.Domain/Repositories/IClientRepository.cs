using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Domain.Repositories
{
    public interface IClientRepository
    {
        /// <summary>
        /// Assumption is that there's only one client (me).
        /// </summary>
        /// <returns></returns>       
        Task<Client> GetClient();

        Task UpdateAuthentication(Client client, string correlationId);
    }
}