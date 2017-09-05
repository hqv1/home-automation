using System.Threading.Tasks;
using Autofac;
using Bogus;
using Hqv.CSharp.Common.Map;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Handlers;
using Hqv.Thermostat.Api.Models;
using Moq;
using Xunit;

namespace Hqv.Thermostat.Api.Tests.Unit
{
    public class GetThermostatReadingHandlerTest
    {
        private readonly GetThermostatReadingHandler _handler;

        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IEventLogger> _eventLogger;
        private Mock<IThermostatProvider> _thermostatProvider;

        private readonly Faker _faker;

        public GetThermostatReadingHandlerTest()
        {
            _faker = new Faker("en");

            var iocContainer = Ioc();
            _handler = iocContainer.Resolve<GetThermostatReadingHandler>();

            GivenAnAuthenticationService();            
        }

        private void GivenAnAuthenticationService()
        {
            _authenticationService
                .Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>()))
                .Returns<AuthenticateRequest>(request =>
                    Task.FromResult(new AuthenticateResponse(request)
                    {
                        ClientId = _faker.Random.Int(),
                        BearerToken = _faker.Random.AlphaNumeric(10)
                    }));
        }


        [Fact]
        public void Should_GetThermostatReading()
        {
            var message = new ReadingToGetModel
            {
                CorrelationId = _faker.Random.AlphaNumeric(15),
                IncludeReadings = true,
                IncludeScenes = true,
                IncludeSettings = true
            };
            var response = _handler.Handle(message);
        }

        private IContainer Ioc()
        {
            var builder = new ContainerBuilder();

            _authenticationService = new Mock<IAuthenticationService>();

            builder.RegisterInstance(_authenticationService.Object).As<IAuthenticationService>();

            _eventLogger = new Mock<IEventLogger>();
            builder.RegisterInstance(_eventLogger.Object).As<IEventLogger>();

            builder.RegisterType<Mapper>().As<IMapper>();

            _thermostatProvider = new Mock<IThermostatProvider>();
            builder.RegisterInstance(_thermostatProvider.Object).As<IThermostatProvider>();

            builder.RegisterType<GetThermostatReadingHandler>();

            return builder.Build();
        }
    }
}
