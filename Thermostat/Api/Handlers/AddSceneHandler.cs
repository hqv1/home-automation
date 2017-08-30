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
    public class AddSceneHandler : IAsyncRequestHandler<SceneToAddModel, object>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IHqvLogger _logger;
        private readonly IMapper _mapper;
        private readonly ISceneProvider _sceneProvider;
        private SceneToAddModel _message;

        public AddSceneHandler(
            IAuthenticationService authenticationService, 
            IEventLogRepository eventLogRepository,
            IHqvLogger logger,
            IMapper mapper,
            ISceneProvider sceneProvider)
        {
            _authenticationService = authenticationService;
            _eventLogRepository = eventLogRepository;
            _logger = logger;
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
            await _eventLogRepository.Add(new EventLog("Scene", "", "SeceneAdded",
                DateTime.UtcNow, _message.CorrelationId, entityObject: _message));
        }

        private async Task StoreExceptionDomainEvent(Exception ex)
        {
            try
            {
                await _eventLogRepository.Add(new EventLog("Scene", "", "SceneAddFailed",
                    DateTime.UtcNow, _message.CorrelationId, entityObject: _message, additionalMetadata: ex));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unable to save domain event for correlation {correlationId}", _message.CorrelationId);
                _logger.Error(ex, "SceneAddFailed with correlation {correlationId}", _message.CorrelationId);
            }
        }
    }
}