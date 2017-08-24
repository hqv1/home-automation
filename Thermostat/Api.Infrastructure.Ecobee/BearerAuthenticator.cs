using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation;
using Hqv.CSharp.Common.Exceptions;
using Hqv.CSharp.Common.Validations;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared;
using Newtonsoft.Json;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee
{
    /// <summary>
    /// https://www.ecobee.com/home/developer/api/examples/ex1.shtml
    /// 
    /// </summary>
    public class BearerAuthenticator : IEcobeeAuthenticator
    {
        private readonly Settings _settings;

        public class Settings
        {         
            public Settings(string baseUri, string tokenUri)
            {
                BaseUri = baseUri;
                TokenUri = tokenUri;

                AccessTokenExpirationFuzzyInSeconds = 300;
                RefreshTokenExpirationFuzzyInMonths = 3;

                Validator.Validate<Settings, SettingsValidator>(this);
            }

            public string BaseUri { get; }
            public string TokenUri { get; }

            public int AccessTokenExpirationFuzzyInSeconds { get; }
            public int RefreshTokenExpirationFuzzyInMonths { get; }
        }

        private class SettingsValidator : AbstractValidator<Settings>
        {
            public SettingsValidator()
            {
                RuleFor(x => x.BaseUri).NotEmpty();
                RuleFor(x => x.TokenUri).NotEmpty();
                RuleFor(x => x.AccessTokenExpirationFuzzyInSeconds).GreaterThanOrEqualTo(0);
                RuleFor(x => x.RefreshTokenExpirationFuzzyInMonths).GreaterThanOrEqualTo(0);
            }
        }

        public BearerAuthenticator(Settings settings)
        {
            _settings = settings;
        }
              
        /// <summary>
        /// Getting access token using refresh token isn't working. After a day the refresh token becomes invalid. 
        /// So we are obtaining both tokens.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task GetBearerToken(Client client)
        {
            var authentication = client.Authentication;
            await GetTokensUsingRefreshToken(authentication);
        }
      
        /// <summary>
        /// Not working. But keeping the code just in case.
        /// </summary>
        private static bool IsValidRefreshToken(ClientAuthentication authentication)
        {
            return !string.IsNullOrEmpty(authentication.RefreshToken) &&
                   authentication.RefreshTokenExpiration > DateTime.Now;
        }

        /// <summary>
        /// Not working. But keeping the code just in case.
        /// </summary>
        private async Task GetAccessTokenUsingRefreshToken(ClientAuthentication authentication)
        {
            var uri = UriHelper.Create(_settings.BaseUri, _settings.TokenUri);
            var parameters = new Dictionary<string, string>()
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", authentication.RefreshToken },
                {"client_id", authentication.AppApiKey }
            };
            var content = new FormUrlEncodedContent(parameters);            
            var client = new HttpClient();
            
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(uri, content);
            }
            catch (Exception ex)
            {
                var exception = new HqvException("Getting access token using refresh tokens failed.", ex);
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                throw exception;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception =
                    new HqvException($"Getting access token using refresh tokens failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseContent);
                var expiresInSeconds = (int)json.expires_in - _settings.AccessTokenExpirationFuzzyInSeconds;
                authentication.SetAccessToken((string)json.access_token,
                    DateTime.Now.AddSeconds(expiresInSeconds).ToUniversalTime());
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

        private async Task GetTokensUsingRefreshToken(ClientAuthentication authentication)
        {
            var parameters = new Dictionary<string, string>()
            {
                {"grant_type", "refresh_token"},
                {"code", authentication.RefreshToken },
                {"client_id", authentication.AppApiKey }
            };
            var uri = UriHelper.Create(_settings.BaseUri, _settings.TokenUri);           

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(parameters);

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(uri, content);
            }
            catch (Exception ex)
            {
                var exception = new HqvException("Getting tokens using refresh tokens failed.", ex);
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                throw exception;
            }

            if (!response.IsSuccessStatusCode)
            {
                var exception =
                    new HqvException($"Getting tokens using refresh tokens failed with error code {response.StatusCode}");
                exception.Data["uri"] = uri;
                exception.Data["request-content"] = await content.ReadAsStringAsync();
                exception.Data["response-content"] = await response.Content.ReadAsStringAsync();
                throw exception;
            }

            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic json = JsonConvert.DeserializeObject(responseContent);
                var expiresInSeconds = (int)json.expires_in - _settings.AccessTokenExpirationFuzzyInSeconds;
                authentication.SetAccessToken((string)json.access_token,
                    DateTime.Now.AddSeconds(expiresInSeconds).ToUniversalTime());
                authentication.SetRefreshToken((string)json.refresh_token,
                    DateTime.Now.AddYears(1).AddMonths(-_settings.RefreshTokenExpirationFuzzyInMonths).ToUniversalTime());
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
