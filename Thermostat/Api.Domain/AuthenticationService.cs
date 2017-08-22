using System;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Exceptions;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;

namespace Hqv.Thermostat.Api.Domain
{   
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEcobeeAuthenticator _ecobeeAuthenticator;
        private readonly IClientRepository _clientRepository;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IHqvLogger _logger;
        private readonly Settings _settings;
        private AuthenticateRequest _request;
        private AuthenticateResponse _response;

        public class Settings
        {
            
        }

        public AuthenticationService(
            IEcobeeAuthenticator ecobeeAuthenticator, 
            IClientRepository clientRepository,             
            IEventLogRepository eventLogRepository,
            IHqvLogger logger,
            Settings settings)
        {
            _ecobeeAuthenticator = ecobeeAuthenticator;
            _clientRepository = clientRepository;
            _eventLogRepository = eventLogRepository;
            _logger = logger;
            _settings = settings;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            _request = request;
            _response = new AuthenticateResponse(_request);
            try
            {
                await Authenticate();                
            }
            catch (HqvException ex)
            {
                _response.AddError(ex);
                await InsertAuthenticationFailed(ex);
            }
            catch (Exception ex)
            {
                var exception = new Exception("Unhandled exception in AuthenticationService", ex);
                _response.AddError(exception);
                await InsertAuthenticationFailed(exception);
            }
            return _response;
        }

        private async Task Authenticate()
        {
            //todo: cache client (redis)

            var client = await _clientRepository.GetClient();
            _response.ClientId = client.ClientId;
            if (client.Authentication.AccessTokenExpiration > DateTime.Now)
            {
                _response.BearerToken = client.Authentication.AccessToken;
                return;
            }

            await _ecobeeAuthenticator.GetBearerToken(client);
            await _clientRepository.UpdateAuthentication(client);
            _response.BearerToken = client.Authentication.AccessToken;
            await _eventLogRepository.Add(new EventLog("Client", Convert.ToString(client.ClientId), "Authenticated",
                DateTime.Now, _request.CorrelationId, entityObject: _response.BearerToken));
        }

        private async Task InsertAuthenticationFailed(Exception ex)
        {
            try
            {
                await _eventLogRepository.Add(new EventLog("Client", Convert.ToString(_response.ClientId), "AuthenticationFailed",
                    DateTime.Now, _request.CorrelationId, additionalMetadata: ex));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unable to save domain event for correlation {correlationId}",
                    _request.CorrelationId);
                _logger.Error(ex, "AuthenticationFailed for Client {clientId} with correlation {correlationId}",
                    _response.ClientId, _request.CorrelationId);
            }            
        }
    }
}