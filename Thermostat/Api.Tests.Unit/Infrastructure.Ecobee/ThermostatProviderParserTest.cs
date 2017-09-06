using System.IO;
using System.Linq;
using FluentAssertions;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers;
using Newtonsoft.Json;
using Xunit;

namespace Hqv.Thermostat.Api.Tests.Unit.Infrastructure.Ecobee
{
    public class ThermostatProviderParserTest
    {
        private const string FilePath = "Infrastructure.Ecobee/TestFiles/ThermostatList.txt";

        [Fact]
        [Trait("Category", "Unit")]
        public void Should_ParseTermostatList()
        {
            var data = File.ReadAllText(FilePath);
            var json = JsonConvert.DeserializeObject(data);

            var thermostats = ThermostatListParser.Parse(json).ToList();
            thermostats.ElementAt(0).Reading.TemperatureInF.Should().Be(769);
            thermostats.ElementAt(0).Settings.HeatRangeHigh.Should().Be(790);
            thermostats.ElementAt(0).Scenes.ElementAt(0).Name.Should().Be("home");
        }
    }
}