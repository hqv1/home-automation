using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Hqv.CSharp.Common.Validations;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee
{
    public class ThermostatProvider : IThermostatProvider
    {
        private readonly Settings _settings;

        public class Settings
        {
            public Settings(string baseUri, string thermostatUri)
            {
                BaseUri = baseUri;
                ThermostatUri = thermostatUri;
                Validator.Validate<Settings, SettingsValidator>(this);
            }

            public string BaseUri { get; }
            public string ThermostatUri { get; }
        }

        private class SettingsValidator : AbstractValidator<Settings>
        {
            public SettingsValidator()
            {
                RuleFor(x => x.BaseUri).NotEmpty();
                RuleFor(x => x.ThermostatUri).NotEmpty();
            }
        }

        public ThermostatProvider(Settings settings)
        {
            _settings = settings;
        }

        public async Task<IEnumerable<Domain.Entities.Thermostat>> GetThermostats(string bearerToken)
        {
            var client = new HttpClient();
            var thermostats = await client.GetAsyncParsed<IEnumerable<Domain.Entities.Thermostat>>(
                baseUri: _settings.BaseUri,
                relativeUri: _settings.ThermostatUri,
                queryParameters: CreateQueryParameters(),
                bearerToken: bearerToken, 
                parser: json => ThermostatListParser.Parse(json)               
            );
            return thermostats;
        }

        private static ICollection<KeyValuePair<string, string>> CreateQueryParameters()
        {
            var body = new
            {
                selection = new
                {
                    selectionType = "registered",
                    selectionMatch = "",
                    includeRuntime = true
                }
            };
            var queryParameters = new Dictionary<string, string>()
            {
                {"format", "json"},
                {"body",  JsonConvert.SerializeObject(body)}
            };
            return queryParameters;
        }
    }
}