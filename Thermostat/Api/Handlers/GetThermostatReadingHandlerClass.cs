using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;
using Hqv.Thermostat.Api.Messages;
using MediatR;

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
            var bearerToken = await GetBearerToken(correlationId);

            IEnumerable<Domain.Entities.Thermostat> thermostats = null;
            try
            {
                thermostats = (await _thermostatProvider.GetThermostats(bearerToken)).ToList();
                // ReSharper disable PossibleMultipleEnumeration
                var model = thermostats.Select(x => new
                {
                    Name = x.Name,
                    Reading = new
                    {
                        ReadingDateTime = x.Reading.DateTime,
                        Temperature = x.Reading.TemperatureInF,
                        Humidity = x.Reading.Humidity
                    }
                });
                return model;
            }
            catch (Exception ex)
            {
                await StoreExceptionDomainEvent(ex, thermostats, correlationId);
                throw;
                // ReSharper restore PossibleMultipleEnumeration
            }
        }

        private async Task<string> GetBearerToken(string correlationId)
        {
            var response = await _authenticationService.Authenticate(new AuthenticateRequest(correlationId));
            if (!response.IsValid)
                throw response.Errors.FirstOrDefault();
            return response.BearerToken;
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
