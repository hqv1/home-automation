using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentValidation;
using Hqv.CSharp.Common.Exceptions;
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
            var queryParameters = CreateQueryParameters();
            var uri = UriHelper.Create(_settings.BaseUri, _settings.ThermostatUri, queryParameters);           
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(uri);
            }
            catch (Exception ex)
            {
                var exception = new HqvException("Getting thermostats failed.", ex);
                exception.Data["uri"] = uri;               
                throw exception;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception =
                    new HqvException($"Getting thermostats failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseContent);
                var thermostats = ThermostatListParser.Parse(json);
                return thermostats;
            }
            catch (Exception ex)
            {
                var exception = new HqvException("Unable to parse result from Ecobee for getting thermostat using refresh tokens", ex);
                exception.Data["uri"] = uri;               
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }
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