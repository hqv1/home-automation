using System;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;

namespace Hqv.Thermostat.Api.Domain.Helpers
{
    public class EventLogger : IEventLogger
    {
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IHqvLogger _logger;

        public EventLogger(IEventLogRepository eventLogRepository, IHqvLogger logger)
        {
            _eventLogRepository = eventLogRepository;
            _logger = logger;
        }

        public async Task AddDomainEvent(EventLog eventLog)
        {
            try
            {
                await _eventLogRepository.Add(eventLog);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unable to save domain event into repository for correlation {CorrelationId}", eventLog.CorrelationId);
                _logger.Error("Event Log {EventLog} with correlation {CorrelationId}", eventLog, eventLog.CorrelationId);
                throw;
            }
        }

        public async Task AddExceptionDomainEvent(EventLog eventLog)
        {
            try
            {
                await _eventLogRepository.Add(eventLog);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unable to save exception domain event into repository for correlation {CorrelationId}", eventLog.CorrelationId);
                _logger.Error("Event Log {EventLog} with correlation {CorrelationId}", eventLog, eventLog.CorrelationId);
            }
        }
    }
}