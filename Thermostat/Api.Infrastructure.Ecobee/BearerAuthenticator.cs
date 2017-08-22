using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Hqv.CSharp.Common.Exceptions;
using Hqv.CSharp.Common.Validations;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee
{
    /// <summary>
    /// https://www.ecobee.com/home/developer/api/examples/ex1.shtml
    /// 
    /// 
    /// </summary>
    public class BearerAuthenticator : IEcobeeAuthenticator
    {
        private readonly Settings _settings;

        public class Settings
        {
            //private const string BaseUri = "https://api.ecobee.com";
            //private const string AuthorizationUri = "authorize";
            //private const string TokenUri = "token";

            public Settings(string baseUri, string authorizationUri, string tokenUri)
            {
                BaseUri = baseUri;
                AuthorizationUri = authorizationUri;
                TokenUri = tokenUri;

                Validator.Validate<Settings, SettingsValidator>(this);
            }

            public string BaseUri { get; }
            public string AuthorizationUri { get; }
            public string TokenUri { get; }
        }

        private class SettingsValidator : AbstractValidator<Settings>
        {
            public SettingsValidator()
            {
                RuleFor(x => x.BaseUri).NotEmpty();
                RuleFor(x => x.AuthorizationUri).NotEmpty();
                RuleFor(x => x.TokenUri).NotEmpty();
            }
        }

        public BearerAuthenticator(Settings settings)
        {
            _settings = settings;
        }
              
        public async Task GetBearerToken(Client client)
        {
            var authentication = client.Authentication;
            if (IsValidRefreshToken(authentication))
            {              
                await GetTokenUsingRefreshToken(authentication);
            }
            else
            {
                throw new HqvException("Refresh token has expired. Need to get new codes");
                //await GetTokensUsingPin(authentication);
            }
        }
      
        private static bool IsValidRefreshToken(ClientAuthentication authentication)
        {
            return !string.IsNullOrEmpty(authentication.RefreshToken) &&
                   authentication.RefreshTokenExpiration > DateTime.Now;
        }

        private async Task GetTokensUsingPin(ClientAuthentication authentication)
        {
            var parameters = new Dictionary<string, string>()
            {
                {"grant_type", "ecobeePin"},
                {"code", authentication.AppAuthorizationCode },
                {"client_id", authentication.AppApiKey }
            };
            var uriBuilder = new UriBuilder(_settings.BaseUri);
            uriBuilder.Path += _settings.TokenUri;
            var uri = uriBuilder.ToString();

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(parameters);

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(uri, content);
            }
            catch (Exception ex)
            {
                var exception = new HqvException("Getting token using PIN failed.", ex);
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                throw exception;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception = new HqvException($"Getting token using PIN failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseContent);

                authentication.SetRefreshToken((string) json.refresh_token,
                    DateTime.Now.AddYears(1).AddMinutes(-1));

                var expiresInSeconds = (int) json.expires_in - 30;
                authentication.SetAccessToken((string) json.access_token,
                    DateTime.Now.AddSeconds(expiresInSeconds));
            }
            catch (Exception ex)
            {
                var exception = new HqvException($"Unable to parse result from Ecobee for getting token using PIN", ex);
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }
        }

        public async Task GetTokenUsingRefreshToken(ClientAuthentication authentication)
        {
            var parameters = new Dictionary<string, string>()
            {
                {"grant_type", "refresh_token"},
                {"code", authentication.RefreshToken },
                {"client_id", authentication.AppApiKey }
            };
            var uriBuilder = new UriBuilder(_settings.BaseUri);
            uriBuilder.Path += _settings.TokenUri;
            var uri = uriBuilder.ToString();

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(parameters);

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(uri, content);
            }
            catch (Exception ex)
            {
                var exception = new HqvException("Getting token using refresh tokens failed.", ex);
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                throw exception;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception =
                    new HqvException($"Getting token using refresh tokens failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseContent);
                var expiresInSeconds = (int)json.expires_in - 30;
                authentication.SetAccessToken((string)json.access_token,
                    DateTime.Now.AddSeconds(expiresInSeconds));
            }
            catch (Exception ex)
            {
                var exception = new HqvException($"Unable to parse result from Ecobee for getting token using refresh tokens", ex);
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }            
        }
    }
    
}
