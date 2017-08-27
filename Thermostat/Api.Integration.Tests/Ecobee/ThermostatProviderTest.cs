using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Infrastructure.Ecobee;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared;
using Xunit;

namespace Hqv.Thermostat.Api.Integration.Tests.Ecobee
{
    public class ThermostatProviderTest
    {
        private const string BaseUri = "https://api.ecobee.com";
        private const string ThermostatUri = "1/thermostat";
        private const string BearerToken = "8nyvmfXsoDZEZZ6ad7NEz6NLomDsMigj";

        private readonly IThermostatProvider _thermostatProvider;

        public ThermostatProviderTest()
        {
            IHqvHttpClient client = new HqvHttpClient(null, new HqvHttpClient.Settings(false));
            var settings = new ThermostatProvider.Settings(BaseUri, ThermostatUri);
            _thermostatProvider = new ThermostatProvider(client, settings);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test()
        {
            var thermostats = _thermostatProvider.GetThermostats(BearerToken).Result;
        }
    }
}