using System;

namespace Hqv.Thermostat.Api.Domain.Entities
{
    public class ClientAuthentication
    {
        public ClientAuthentication(
            string appApiKey, string appAuthorizationCode, 
            string refreshToken = null, DateTime? refreshTokenExpiration = null, 
            string accessToken = null, DateTime? accessTokenExpiration = null)
        {

            AppApiKey = appApiKey;
            AppAuthorizationCode = appAuthorizationCode;
            RefreshToken = refreshToken;
            RefreshTokenExpiration = refreshTokenExpiration;
            AccessToken = accessToken;
            AccessTokenExpiration = accessTokenExpiration;
        }
        
        public string AppApiKey { get; }
        public string AppAuthorizationCode { get; }
        public string RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiration { get; private set; }
        public string AccessToken { get; private set; }
        public DateTime? AccessTokenExpiration { get; private set; }

        public void SetRefreshToken(string refreshToken, DateTime refreshTokenExpiration)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpiration = refreshTokenExpiration;
        }

        public void SetAccessToken(string accessToken, DateTime accessTokenExpiration)
        {
            AccessToken = accessToken;
            AccessTokenExpiration = accessTokenExpiration;
        }
    }
}