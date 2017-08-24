using System;
using FluentAssertions;
using Hqv.Thermostat.Api.Infrastructure;
using Hqv.Thermostat.Api.Infrastructure.Repositories;
using Xunit;

namespace Hqv.Thermostat.Api.Integration.Tests
{
    public class ClientRepositoryTest
    {
        private const string ConnectionString = "Server=tcp:127.0.0.1,5433;Initial Catalog=Hqv.Thermostat;User Id=sa;Password=Pass@word";
        private readonly ClientRepository _clientRepository;

        public ClientRepositoryTest()
        {           
            var eventLogRepository = new EventLogRepository(new EventLogRepository.Settings(ConnectionString));

            _clientRepository = new ClientRepository(
                eventLogRepository,
                new ClientRepository.Settings(ConnectionString));
            
        }

        [Fact]        
        [Trait("Category", "Integration")]
        public void Should_GetClient()
        {
            var client = _clientRepository.GetClient().Result;
            client.Name.Should().Be("Me");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Should_UpdateAuthentication()
        {
            var client = _clientRepository.GetClient().Result;
            client.Authentication.SetRefreshToken("FakeToken", DateTime.Now);

            _clientRepository.UpdateAuthentication(client).Wait();
        }
    }
}
