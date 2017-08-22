using System.Threading.Tasks;

namespace Hqv.Thermostat.Administrator
{
    public interface ISystemRepository
    {
        Task CreateDatabase();
    }
}