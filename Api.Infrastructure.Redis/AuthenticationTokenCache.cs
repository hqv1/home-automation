using System;
using StackExchange.Redis;

namespace Hqv.Thermostat.Api.Infrastructure.Redis
{
    public class AuthenticationTokenCache
    {
        private const string AuthenticationTokenPrefix = "Auth_";
        private readonly TimeSpan _cacheTimeSpan;

        private readonly ConnectionMultiplexer _redis;        

        public AuthenticationTokenCache(RedisConnection connection)
        {
            _redis = connection.Connection;
            _cacheTimeSpan = TimeSpan.FromMinutes(50);
        }

        public string GetAuthenticatonToken(string clientId)
        {
            var db = _redis.GetDatabase();
            return db.StringGet(AuthenticationTokenPrefix + clientId);
        }

        public void SetAuthenticatonToken(string clientId, string token)
        {
            var db = _redis.GetDatabase();
            db.StringSet(AuthenticationTokenPrefix + clientId, token, _cacheTimeSpan);
        }
    }
}
