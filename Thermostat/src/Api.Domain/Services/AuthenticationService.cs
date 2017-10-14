using System;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Exceptions;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;

namespace Hqv.Thermostat.Api.Domain.Services
{   
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationCache _authenticationCache;
        private readonly IClientRepository _clientRepository;
        private readonly IEcobeeAuthenticator _ecobeeAuthenticator;
        private readonly IEventLogger _eventLogger;
        private AuthenticateRequest _request;
        private AuthenticateResponse _response;

        public AuthenticationService(
            IAuthenticationCache authenticationCache,
            IClientRepository clientRepository,
            IEcobeeAuthenticator ecobeeAuthenticator,
            IEventLogger eventLogger)
        {
            _authenticationCache = authenticationCache;
            _clientRepository = clientRepository;
            _ecobeeAuthenticator = ecobeeAuthenticator;
            _eventLogger = eventLogger;
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
            var client = await GetFromCache() ?? await GetFromDatabase();

            _response.ClientId = client.ClientId;
            _response.BearerToken = client.Authentication.AccessToken;
        }

        private async Task InsertAuthenticationFailed(Exception ex)
        {                       
            await _eventLogger.AddExceptionDomainEvent(new EventLog("Client", Convert.ToString(_response.ClientId),
                "AuthenticationFailed", DateTime.Now, _request.CorrelationId, additionalMetadata: ex));
        }

        private async Task<Client> GetFromCache()
        {
            var client = _authenticationCache.GetToken();
            if (client == null) return null;
            
            await _eventLogger.AddDomainEvent(new EventLog("Client", Convert.ToString(0), "Get client from cache",
                DateTime.Now, _request.CorrelationId, entityObject: _response.BearerToken));
            return client;
        }

        private async Task<Client> GetFromDatabase()
        {
            var client = await _clientRepository.GetClient();
                      
            if (client.Authentication.AccessTokenExpiration > DateTime.Now.ToUniversalTime())
            {
                _authenticationCache.SetToken(client);
                return client;
            }

            await _ecobeeAuthenticator.GetBearerToken(client);
            await _clientRepository.UpdateAuthentication(client, _request.CorrelationId);
            _authenticationCache.SetToken(client);
          
            await _eventLogger.AddDomainEvent(new EventLog("Client", Convert.ToString(client.ClientId), "Authenticated",
                DateTime.Now, _request.CorrelationId, entityObject: _response.BearerToken));

            return client;
        }
    }
}