using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Hqv.CSharp.Common.Validations;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Models;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Parsers;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee
{
    public class SceneProvider : ISceneProvider
    {
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IHqvHttpClient _httpClient;
        private readonly Settings _settings;
        private string _correlationId;

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

        public SceneProvider(IEventLogRepository eventLogRepository, IHqvHttpClient httpClient, Settings settings)
        {
            _eventLogRepository = eventLogRepository;
            _httpClient = httpClient;
            _settings = settings;
        }

        public async Task AddScene(Scene scene, string bearerToken, string correlationId = null)
        {
            _correlationId = correlationId;
            var queryParameters = CreateAddSceneQueryParameter(scene);
            const string body = "";

            var response = await _httpClient.PostAsyncJsonWithBearerToken<object>(
                baseUri: _settings.BaseUri,
                relativeUri: _settings.ThermostatUri,
                queryParameters: queryParameters,
                bearerToken: bearerToken,
                body: body,
                parser: async json => await Parse(json));
        }

        private static ICollection<KeyValuePair<string, string>> CreateAddSceneQueryParameter(Scene scene)
        {
            var body = new
            {
                selection = new
                {
                    selectionType = "registered",
                    selectionMatch = ""
                },
                functions = new[]
                {
                    new
                    {
                        type = "setHold",
                        @params = new
                        {
                            holdType = "nextTransition",
                            heatHoldTemp = scene.HeatHoldTemp,
                            coolHoldTemp = scene.CoolHoldTemp
                        }
                    }
                }
            };

            return new Dictionary<string, string>()
            {
                {"format", "json"},
                {"body",  JsonConvert.SerializeObject(body)}
            };
        }
        
        private async Task<EcobeeResponsesModel> Parse(object json)
        {
            if (_settings.StoreResponse)
            {
                await _eventLogRepository.Add(new EventLog(
                    "Ecobee", _settings.ThermostatUri, "ResponseBody", DateTime.UtcNow, _correlationId, entityObject: json));
            }
            return EcobeeResponseParser.Parse(json);
        }

        public Task RemoveAllScenes(string bearerToken, string correlationId = null)
        {
            throw new System.NotImplementedException();
        }
    }
}