using StackExchange.Redis;

namespace Hqv.Thermostat.Api.Infrastructure.Redis
{
    public sealed class RedisConnection
    {
        public ConnectionMultiplexer Connection { get; }

        public RedisConnection(string connectionString)
        {
            Connection = ConnectionMultiplexer.Connect(connectionString);
        }
    }
}