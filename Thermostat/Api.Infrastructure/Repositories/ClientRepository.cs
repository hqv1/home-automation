using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Hqv.CSharp.Common.Exceptions;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Domain.Repositories;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Hqv.Thermostat.Api.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        public class Settings
        {
            public Settings(string connectionString)
            {
                ConnectionString = connectionString;
            }
            public string ConnectionString { get; }
        }

        private readonly IEventLogDatabaseRepository _eventLogRepository;
        private readonly Settings _settings;

        public ClientRepository(IEventLogDatabaseRepository eventLogRepository, Settings settings)
        {
            _eventLogRepository = eventLogRepository;
            _settings = settings;
        }

        public async Task<Client> GetClient()
        {
            const string command = "SELECT * FROM [Thermostat].[Client] CROSS JOIN [Thermostat].[Application]";
            dynamic result = null;

            try
            {
                using (var connection = new SqlConnection(_settings.ConnectionString))
                {
                    result = (await connection.QueryAsync(command)).FirstOrDefault();
                }

                return new Client(
                    Convert.ToInt64(result.ClientID),
                    Convert.ToString(result.Name),
                    new ClientAuthentication(
                        Convert.ToString(result.ApiKey),
                        Convert.ToString(result.AuthorizationCode),
                        Convert.ToString(result.RefreshToken),
                        result.RefreshTokenExpiration == null ? null : Convert.ToDateTime(result.RefreshTokenExpiration),
                        Convert.ToString(result.AccessToken),
                        result.AccessTokenExpiration == null ? null : Convert.ToDateTime(result.AccessTokenExpiration))
                );
            }
            catch (Exception ex)
            {
                var exception = new HqvException($"Unable to get client with command {command}", ex);
                if (result != null) exception.Data["result"] = result;
                throw exception;
            }            
        }

        public async Task UpdateAuthentication(Client client, string correlationId = null)
        {
            const string command =
                @"UPDATE [Thermostat].[Client] SET RefreshToken = @RefreshToken, RefreshTokenExpiration = @RefreshTokenExpiration, AccessToken= @AccessToken, AccessTokenExpiration = @AccessTokenExpiration WHERE ClientID = @ClientID";

            using (var connection = new SqlConnection(_settings.ConnectionString))
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = await connection.ExecuteAsync(command, new
                {
                    ClientID = client.ClientId,
                    RefreshToken = client.Authentication.RefreshToken,
                    RefreshTokenExpiration = client.Authentication.RefreshTokenExpiration,
                    AccessToken = client.Authentication.AccessToken,
                    AccessTokenExpiration = client.Authentication.AccessTokenExpiration
                });

                var eventLog = new EventLog("Client", client.ClientId.ToString(), "AuthenticationUpdated",
                    DateTime.Now.ToUniversalTime(), correlationId, entityObject: client);
                await _eventLogRepository.Add(eventLog, connection);

                transaction.Complete();
            }
          
        }
    }
}
