using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;
using Hqv.Thermostat.Api.Extensions;
using Hqv.Thermostat.Api.Messages;
using MediatR;
// ReSharper disable PossibleMultipleEnumeration

namespace Hqv.Thermostat.Api.Handlers
{
    public class GetThermostatReadingHandlerClass : IAsyncRequestHandler<GetThermostatReadingMessage, object>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IHqvLogger _logger;
        private readonly IThermostatProvider _thermostatProvider;

        public GetThermostatReadingHandlerClass(
            IAuthenticationService authenticationService,
            IEventLogRepository eventLogRepository,
            IHqvLogger logger,
            IThermostatProvider thermostatProvider)
        {
            _authenticationService = authenticationService;
            _eventLogRepository = eventLogRepository;
            _logger = logger;
            _thermostatProvider = thermostatProvider;
        }

        public async Task<object> Handle(GetThermostatReadingMessage message)
        {
            var correlationId = Guid.NewGuid().ToString();
            var bearerToken = await _authenticationService.GetBearerToken(correlationId);

            IEnumerable<Domain.Entities.Thermostat> thermostats = null;
            try
            {
                thermostats = (await _thermostatProvider.GetThermostats(bearerToken, correlationId)).ToList();              
                var model = thermostats.Select(Transform);
                await StoreDomainEvent(correlationId);
                return model;
            }
            catch (Exception ex)
            {
                await StoreExceptionDomainEvent(ex, thermostats, correlationId);
                throw;
            }
        }

        private static object Transform(Domain.Entities.Thermostat thermostat)
        {
            return new
            {
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

        private async Task StoreDomainEvent(string correlationId)
        {
            await _eventLogRepository.Add(new EventLog("Thermostat", "", "GetReading", DateTime.Now.ToUniversalTime(),
                correlationId));
        }

        private async Task StoreExceptionDomainEvent(Exception ex, IEnumerable<Domain.Entities.Thermostat> thermostats, string correlationId)
        {
            try
            {
                await _eventLogRepository.Add(new EventLog("Thermostat", "", "GetReadingFailed",
                    DateTime.Now, correlationId, entityObject: thermostats, additionalMetadata: ex));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unable to save domain event for correlation {correlationId}", correlationId);
                _logger.Error(ex, "GetReadingFailed with correlation {correlationId}", correlationId);
            }
        }
    }
}
