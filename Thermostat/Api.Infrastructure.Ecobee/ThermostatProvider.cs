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
        private readonly IHqvHttpClient _httpClient;
        private readonly Settings _settings;

        public class Settings
        {
            public Settings(string baseUri, string thermostatUri, bool storeResponse = false)
            {
                BaseUri = baseUri;
                ThermostatUri = thermostatUri;
                StoreResponse = storeResponse;
                Validator.Validate<Settings, SettingsValidator>(this);
            }

            public string BaseUri { get; }
            public string ThermostatUri { get; }
            public bool StoreResponse { get; }
        }

        private class SettingsValidator : AbstractValidator<Settings>
        {
            public SettingsValidator()
            {
                RuleFor(x => x.BaseUri).NotEmpty();
                RuleFor(x => x.ThermostatUri).NotEmpty();
            }
        }

        public ThermostatProvider(IHqvHttpClient httpClient, Settings settings)
        {
            _httpClient = httpClient;
            _settings = settings;
        }

        public async Task<IEnumerable<Domain.Entities.Thermostat>> GetThermostats(string bearerToken, string correlationId = null)
        {
            var thermostats = await _httpClient.GetAsyncWithBearerToken(
                correlationId: correlationId,
                baseUri: _settings.BaseUri,
                relativeUri: _settings.ThermostatUri,
                queryParameters: CreateQueryParameters(),
                bearerToken: bearerToken,
                parser: json => ThermostatListParser.Parse(json));

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
                    includeRuntime = true,
                    includeSettings = true //just added
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