using Hqv.Thermostat.Api.Infrastructure.Ecobee;
using Xunit;

namespace Hqv.Thermostat.Api.Integration.Tests.Ecobee
{
    public class ThermostatProviderTest
    {
        private const string BaseUri = "https://api.ecobee.com";
        private const string ThermostatUri = "1/thermostat";
        private const string BearerToken = "8nyvmfXsoDZEZZ6ad7NEz6NLomDsMigj";

        private readonly ThermostatProvider _thermostatProvider;

        public ThermostatProviderTest()
        {
            var settings = new ThermostatProvider.Settings(BaseUri, ThermostatUri);
            _thermostatProvider = new ThermostatProvider(settings);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test()
        {
            var thermostats = _thermostatProvider.GetThermostats(BearerToken).Result;
        }
    }
}