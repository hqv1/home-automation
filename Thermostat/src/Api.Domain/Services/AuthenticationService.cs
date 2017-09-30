using System;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Exceptions;
using Hqv.CSharp.Common.Logging;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;

namespace Hqv.Thermostat.Api.Domain.Services
{   
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IEcobeeAuthenticator _ecobeeAuthenticator;
        private readonly IEventLogger _eventLogger;
        private AuthenticateRequest _request;
        private AuthenticateResponse _response;

        public AuthenticationService(
            IClientRepository clientRepository,
            IEcobeeAuthenticator ecobeeAuthenticator,
            IEventLogger eventLogger)
        {
            _ecobeeAuthenticator = ecobeeAuthenticator;
            _eventLogger = eventLogger;
            _clientRepository = clientRepository;
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
            if (client.Authentication.AccessTokenExpiration > DateTime.Now.ToUniversalTime())
            {
                _response.BearerToken = client.Authentication.AccessToken;
                return;
            }

            await _ecobeeAuthenticator.GetBearerToken(client);
            await _clientRepository.UpdateAuthentication(client, _request.CorrelationId);

            _response.BearerToken = client.Authentication.AccessToken;
            await _eventLogger.AddDomainEvent(new EventLog("Client", Convert.ToString(client.ClientId), "Authenticated",
                DateTime.Now, _request.CorrelationId, entityObject: _response.BearerToken));
        }

        private async Task InsertAuthenticationFailed(Exception ex)
        {
            await _eventLogger.AddExceptionDomainEvent(new EventLog("Client", Convert.ToString(_response.ClientId),
                "AuthenticationFailed", DateTime.Now, _request.CorrelationId, additionalMetadata: ex));
        }
    }
}