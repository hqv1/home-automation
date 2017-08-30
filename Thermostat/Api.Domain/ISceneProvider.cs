using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain.Entities;

namespace Hqv.Thermostat.Api.Domain
{
    public interface ISceneProvider
    {
        Task AddScene(Scene scene, string bearerToken, string correlationId = null);
        Task RemoveAllScenes(string bearerToken, string correlationId = null);
    }
}