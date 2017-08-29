using System.IO;
using System.Linq;
using FluentAssertions;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers;
using Newtonsoft.Json;
using Xunit;

namespace Hqv.Thermostat.Api.Integration.Tests.Ecobee
{
    public class ThermostatProviderParserTest
    {
        private const string FilePath = "TestFiles/ThermostatList.txt";

        [Fact]
        [Trait("Category", "Unit")]
        public void Should_ParseTermostatList()
        {
            var data = File.ReadAllText(FilePath);
            var json = JsonConvert.DeserializeObject(data);

            var thermostats = ThermostatListParser.Parse(json).ToList();
            thermostats.ElementAt(0).Reading.TemperatureInF.Should().Be(769);
            thermostats.ElementAt(0).Settings.HeatRangeHigh.Should().Be(790);
        }
    }
}