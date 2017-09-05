using Hqv.CSharp.Common.Clients;
using Hqv.CSharp.Common.Web.Client;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Dtos;
using Hqv.Thermostat.Api.Infrastructure.Ecobee;

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
            IHqvHttpClient client = new HqvHttpClient();
            var settings = new ThermostatProvider.Settings(BaseUri, ThermostatUri, storeResponse:false);
            _thermostatProvider = new ThermostatProvider(null, client, settings);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Test()
        {
            var request = new GetThermostatsRequest(true,true, true)
            {
                BearerToken = BearerToken
            };
            var thermostats = _thermostatProvider.GetThermostats(request).Result;
        }
    }
}