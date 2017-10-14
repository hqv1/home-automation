using System;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Hqv.Thermostat.Api.Infrastructure.Redis
{
    public class AuthenticationCache : IAuthenticationCache
    {
        private const string AuthenticationTokenPrefix = "Auth_";
        private readonly ConnectionMultiplexer _redis;
        private readonly TimeSpan _cacheTimeSpan;

        public class Settings
        {
            public Settings(int authenticationTokenTimeoutInSecs)
            {
                AuthenticationTokenTimeoutInSecs = authenticationTokenTimeoutInSecs;
            }
            public int AuthenticationTokenTimeoutInSecs { get; }
        }        

        public AuthenticationCache(RedisConnection connection, Settings settings)
        {
            _redis = connection.Connection;
            _cacheTimeSpan = TimeSpan.FromSeconds(settings.AuthenticationTokenTimeoutInSecs);
        }

        public Client GetToken()
        {
            var db = _redis.GetDatabase();
            var str = db.StringGet(AuthenticationTokenPrefix);          
            return string.IsNullOrEmpty(str) ? null : JsonConvert.DeserializeObject<Client>(str);
        }

        public void SetToken(Client client)
        {
            var db = _redis.GetDatabase();
            db.StringSet(AuthenticationTokenPrefix, JsonConvert.SerializeObject(client), _cacheTimeSpan);
        }
    }
}
