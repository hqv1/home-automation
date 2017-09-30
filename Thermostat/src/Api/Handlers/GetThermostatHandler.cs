using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Map;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Dtos;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Extensions;
using Hqv.Thermostat.Api.Models;
using MediatR;
// ReSharper disable PossibleMultipleEnumeration

namespace Hqv.Thermostat.Api.Handlers
{
    public class GetThermostatHandler : IAsyncRequestHandler<ThermostatToGetModel, IEnumerable<ThermostatModel>>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventLogger _eventLogger;
        private readonly IMapper _mapper;
        private readonly IThermostatProvider _thermostatProvider;
        private ThermostatToGetModel _message;

        public GetThermostatHandler(
            IAuthenticationService authenticationService, 
            IEventLogger eventLogger,
            IMapper mapper,
            IThermostatProvider thermostatProvider)
        {
            _authenticationService = authenticationService;
            _eventLogger = eventLogger;
            _mapper = mapper;
            _thermostatProvider = thermostatProvider;
        }

        public async Task<IEnumerable<ThermostatModel>> Handle(ThermostatToGetModel message)
        {
            _message = message;
            
            IEnumerable<Domain.Entities.Thermostat> thermostats = null;
            try
            {
                var bearerToken = await _authenticationService.GetBearerToken(message.CorrelationId);
                var request = CreateGetThermostatsRequest(message, bearerToken);                
                thermostats = (await _thermostatProvider.GetThermostats(request)).ToList();              
                var models = thermostats.Select(Transform);
                await StoreDomainEvent();
                return models;
            }
            catch (Exception ex)
            {              
                await StoreExceptionDomainEvent(ex, thermostats);
                throw;
            }
        }

        private GetThermostatsRequest CreateGetThermostatsRequest(ThermostatToGetModel message, string bearerToken)
        {
            var request = _mapper.Map<GetThermostatsRequest>(message);
            request.BearerToken = bearerToken;
            return request;
        }

        private ThermostatModel Transform(Domain.Entities.Thermostat thermostat)
        {           
            var model = new ThermostatModel
            {
                CorrelationId = _message.CorrelationId,
                Name = thermostat.Name
            };

            if (_message.IncludeReadings)
                model.Reading = _mapper.Map<ThermostatReadingModel>(thermostat.Reading);

            if (_message.IncludeSettings)
                model.Settings = _mapper.Map<ThermostatSettingsModel>(thermostat.Settings);

            if (_message.IncludeScenes)
                model.Scenes = _mapper.Map<IEnumerable<ThermostatSceneModel>>(thermostat.Scenes);

            return model;
        }

        private async Task StoreDomainEvent()
        {
            await _eventLogger.AddDomainEvent(new EventLog("Thermostat", "", "GotReading", DateTime.Now.ToUniversalTime(),
                _message.CorrelationId));
        }

        private async Task StoreExceptionDomainEvent(Exception ex, IEnumerable<Domain.Entities.Thermostat> thermostats)
        {
            await _eventLogger.AddExceptionDomainEvent(new EventLog("Thermostat", "", "GetReadingFailed",
                DateTime.Now, _message.CorrelationId,
                entityObject: thermostats, additionalMetadata: new {Message = _message, Exception = ex}));
        }
    }
}
