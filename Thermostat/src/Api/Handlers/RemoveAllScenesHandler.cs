using System;
using System.Threading.Tasks;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Extensions;
using Hqv.Thermostat.Api.Models;
using MediatR;

namespace Hqv.Thermostat.Api.Handlers
{
    public class RemoveAllScenesHandler : IAsyncRequestHandler<ScenesAllToRemoveModel, object>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventLogger _eventLogger;
        private readonly ISceneProvider _sceneProvider;
        private ScenesAllToRemoveModel _message;

        public RemoveAllScenesHandler(
            IAuthenticationService authenticationService,
            IEventLogger eventLogger,
            ISceneProvider sceneProvider)
        {
            _authenticationService = authenticationService;
            _eventLogger = eventLogger;
            _sceneProvider = sceneProvider;
        }
        public async Task<object> Handle(ScenesAllToRemoveModel message)
        {
            _message = message;
            var bearerToken = await _authenticationService.GetBearerToken(message.CorrelationId);
            try
            {           
                await _sceneProvider.RemoveAllScenes(bearerToken, message.CorrelationId);
                await StoreDomainEvent();
                return "";
            }
            catch (Exception ex)
            {
                await StoreExceptionDomainEvent(ex);
                throw;
            }
        }

        private async Task StoreDomainEvent()
        {
            await _eventLogger.AddDomainEvent(new EventLog("Scene", "", "ScenesRemoved",
                DateTime.UtcNow, _message.CorrelationId, entityObject: _message));
        }

        private async Task StoreExceptionDomainEvent(Exception ex)
        {
            await _eventLogger.AddExceptionDomainEvent(new EventLog("Scene", "", "ScenesRemoveFailed",
                DateTime.UtcNow, _message.CorrelationId, entityObject: _message, additionalMetadata: ex));
        }
    }
}