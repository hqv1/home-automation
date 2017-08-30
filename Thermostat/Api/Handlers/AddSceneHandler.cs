using System;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Logging;
using Hqv.CSharp.Common.Map;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;
using Hqv.Thermostat.Api.Extensions;
using Hqv.Thermostat.Api.Models;
using MediatR;

namespace Hqv.Thermostat.Api.Handlers
{
    public class RemoveAllScenesHandler : IAsyncRequestHandler<ScenesAllToRemoveModel, object>
    {
        public RemoveAllScenesHandler( IAuthenticationService authenticationService)
        {
            
        }
        public Task<object> Handle(ScenesAllToRemoveModel message)
        {
            throw new NotImplementedException();
        }
    }

    public class AddSceneHandler : IAsyncRequestHandler<SceneToAddModel, object>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventLogger _eventLogger;
        private readonly IMapper _mapper;
        private readonly ISceneProvider _sceneProvider;
        private SceneToAddModel _message;

        public AddSceneHandler(
            IAuthenticationService authenticationService,
            IEventLogger eventLogger,
            IMapper mapper,
            ISceneProvider sceneProvider)
        {
            _authenticationService = authenticationService;
            _eventLogger = eventLogger;
            _mapper = mapper;
            _sceneProvider = sceneProvider;
        }

        public async Task<object> Handle(SceneToAddModel message)
        {
            _message = message;
            var bearerToken = await _authenticationService.GetBearerToken(message.CorrelationId);
            try
            {

                var scene = _mapper.Map<Scene>(message);
                await _sceneProvider.AddScene(scene, bearerToken, message.CorrelationId);
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
            await _eventLogger.AddDomainEvent(new EventLog("Scene", "", "SceneAdded",
                DateTime.UtcNow, _message.CorrelationId, entityObject: _message));
        }

        private async Task StoreExceptionDomainEvent(Exception ex)
        {
            await _eventLogger.AddExceptionDomainEvent(new EventLog("Scene", "", "SceneAddFailed",
                DateTime.UtcNow, _message.CorrelationId, entityObject: _message, additionalMetadata: ex));
        }
    }
}