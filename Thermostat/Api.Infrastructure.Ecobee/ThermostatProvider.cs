using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Hqv.CSharp.Common.Clients;
using Hqv.CSharp.Common.Validations;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Dtos;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee
{
    public class ThermostatProvider : IThermostatProvider
    {
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IHqvHttpClient _httpClient;
        private readonly Settings _settings;
        private GetThermostatsRequest _request;

        public class Settings
        {
            public Settings(string baseUri, string thermostatUri, bool storeResponse)
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

        public ThermostatProvider(IEventLogRepository eventLogRepository, IHqvHttpClient httpClient, Settings settings)
        {
            _eventLogRepository = eventLogRepository;
            _httpClient = httpClient;
            _settings = settings;
        }

        public async Task<IEnumerable<Domain.Entities.Thermostat>> GetThermostats(GetThermostatsRequest request)
        {
            _request = request;
            var thermostats = await _httpClient.GetAsyncWithBearerToken(
                baseUri: _settings.BaseUri,
                relativeUri: _settings.ThermostatUri,
                queryParameters: CreateQueryParameters(),
                bearerToken: _request.BearerToken,
                parser: async json => await Parse(json));

            return thermostats;
        }

        private ICollection<KeyValuePair<string, string>> CreateQueryParameters()
        {
            var body = new
            {
                selection = new
                {
                    selectionType = "registered",
                    selectionMatch = "",
                    includeRuntime = _request.IncludeReadings || _request.IncludeSettings,
                    includeSettings = _request.IncludeSettings,
                    includeEvents = _request.IncludeScenes
                }
            };
            var queryParameters = new Dictionary<string, string>()
            {
                {"format", "json"},
                {"body",  JsonConvert.SerializeObject(body)}
            };
            return queryParameters;
        }

        private async Task<IEnumerable<Domain.Entities.Thermostat>> Parse(object json)
        {
            if (_settings.StoreResponse)
            {
                await _eventLogRepository.Add(new EventLog(
                    "Ecobee", _settings.ThermostatUri, "ResponseBody", DateTime.UtcNow, _request.CorrelationId, entityObject: json));
            }

            return ThermostatListParser.Parse(json);
        }
        
    }
}