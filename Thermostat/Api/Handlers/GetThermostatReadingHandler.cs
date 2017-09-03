﻿using System;
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
    public class GetThermostatReadingHandler : IAsyncRequestHandler<ReadingToGetModel, object>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventLogger _eventLogger;
        private readonly IMapper _mapper;
        private readonly IThermostatProvider _thermostatProvider;
        private ReadingToGetModel _message;

        public GetThermostatReadingHandler(
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

        public async Task<object> Handle(ReadingToGetModel message)
        {
            _message = message;
            var bearerToken = await _authenticationService.GetBearerToken(message.CorrelationId);

            IEnumerable<Domain.Entities.Thermostat> thermostats = null;
            try
            {
                var request = _mapper.Map<GetThermostatsRequest>(message);
                request.BearerToken = bearerToken;                
                thermostats = (await _thermostatProvider.GetThermostats(request)).ToList();              
                var model = thermostats.Select(Transform);
                await StoreDomainEvent();
                return model;
            }
            catch (Exception ex)
            {              
                await StoreExceptionDomainEvent(ex, thermostats);
                throw;
            }
        }

        private object Transform(Domain.Entities.Thermostat thermostat)
        {
            return new
            {
                _message.CorrelationId,
                thermostat.Name,
                Reading = _message.IncludeReadings ? new
                {
                    ReadingDateTime = thermostat.Reading.DateTime,
                    Temperature = thermostat.Reading.TemperatureInF,
                    thermostat.Reading.Humidity,
                } : null,
                Settings = _message.IncludeSettings ? new
                {
                    thermostat.Settings.HvacMode,
                    thermostat.Settings.DesiredHeat,
                    thermostat.Settings.DesiredCool,
                    thermostat.Settings.HeatRangeHigh,
                    thermostat.Settings.HeatRangeLow,
                    thermostat.Settings.CoolRangeHigh,
                    thermostat.Settings.CoolRangeLow,
                    thermostat.Settings.HeatCoolMinDelta
                } : null,
                Scenes = _message.IncludeScenes ? thermostat.Scenes.Select(s=> new
                {
                    s.Type,
                    s.Name,
                    s.Running,
                    s.CoolHoldTemp,
                    s.HeatHoldTemp
                }) : null
            };
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
