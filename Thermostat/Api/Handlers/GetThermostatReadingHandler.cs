using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain;
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
        private readonly IThermostatProvider _thermostatProvider;
        private ReadingToGetModel _message;

        public GetThermostatReadingHandler(
            IAuthenticationService authenticationService, 
            IEventLogger eventLogger,
            IThermostatProvider thermostatProvider)
        {
            _authenticationService = authenticationService;
            _eventLogger = eventLogger;
            _thermostatProvider = thermostatProvider;
        }

        public async Task<object> Handle(ReadingToGetModel message)
        {
            _message = message;
            var bearerToken = await _authenticationService.GetBearerToken(message.CorrelationId);

            IEnumerable<Domain.Entities.Thermostat> thermostats = null;
            try
            {
                thermostats = (await _thermostatProvider.GetThermostats(bearerToken, message.CorrelationId)).ToList();              
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
                CorrelationId = _message.CorrelationId,
                Name = thermostat.Name,
                Reading = new
                {
                    ReadingDateTime = thermostat.Reading.DateTime,
                    Temperature = thermostat.Reading.TemperatureInF,
                    thermostat.Reading.Humidity,
                },
                Settings = new
                {
                    thermostat.Settings.HvacMode,
                    thermostat.Settings.DesiredHeat,
                    thermostat.Settings.DesiredCool,
                    thermostat.Settings.HeatRangeHigh,
                    thermostat.Settings.HeatRangeLow,
                    thermostat.Settings.CoolRangeHigh,
                    thermostat.Settings.CoolRangeLow,
                    thermostat.Settings.HeatCoolMinDelta
                },
                Scenes = thermostat.Scenes.Select(s=> new
                {
                    s.Type,
                    s.Name,
                    s.Running,
                    s.CoolHoldTemp,
                    s.HeatHoldTemp
                })
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
