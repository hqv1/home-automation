using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Bogus;
using FluentAssertions;
using Hqv.CSharp.Common.Exceptions;
using Hqv.CSharp.Common.Map;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Dtos;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Handlers;
using Hqv.Thermostat.Api.Models;
using Moq;
using Xunit;

namespace Hqv.Thermostat.Api.Tests.Unit
{
    public class GetThermostatHandlerTest
    {
        private readonly GetThermostatHandler _handler;

        private Mock<IAuthenticationService> _authenticationService;
        private Mock<IEventLogger> _eventLogger;
        private Mock<IThermostatProvider> _thermostatProvider;     
        private Domain.Entities.Thermostat _thermostatToMock;

        private ThermostatToGetModel _message;
        private ThermostatModel _thermostatFromResponse;

        private readonly Faker _faker;

        public GetThermostatHandlerTest()
        {
            _faker = new Faker("en");

            var iocContainer = Ioc();
            _handler = iocContainer.Resolve<GetThermostatHandler>();          
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Should_GetThermostat_WithAllInformation()
        {
            Given_ThermostatToGetModel();
            Given_AuthenticationService();
            Given_Thermostats();
            When_HandleIsCalled();

            Then_ThermostatInformation_IsCorrect();
            _thermostatFromResponse.Reading.Should().NotBeNull();
            _thermostatFromResponse.Settings.Should().NotBeNull();
            _thermostatFromResponse.Scenes.Any().Should().BeTrue();

            _eventLogger.Verify(x=>x.AddDomainEvent(It.IsAny<EventLog>()), Times.Once());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Should_GetThermostat_WithOnlySettings()
        {
            Given_ThermostatToGetModel(includeReadings:false, includeScenes:false);
            Given_AuthenticationService();
            Given_Thermostats();
            When_HandleIsCalled();

            Then_ThermostatInformation_IsCorrect();
            _thermostatFromResponse.Reading.Should().BeNull();
            _thermostatFromResponse.Settings.Should().NotBeNull();
            _thermostatFromResponse.Scenes.Any().Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void Should_LogError_WhenAuthenticationServiceThrows()
        {
            Given_ThermostatToGetModel();
            Given_AuthenticationService_ThatReturnsError();

            var exception = Record.Exception(() => When_HandleIsCalled());
            exception.Should().NotBeNull();
            exception.Should().BeOfType<AggregateException>();

            _eventLogger.Verify(x=>x.AddExceptionDomainEvent(It.IsAny<EventLog>()), Times.Once);
        }

        private void Given_ThermostatToGetModel(
            bool includeReadings = true,
            bool includeScenes = true,
            bool includeSettings = true)
        {
            _message = new ThermostatToGetModel
            {
                CorrelationId = _faker.Random.AlphaNumeric(15),
                IncludeReadings = includeReadings,
                IncludeScenes = includeScenes,
                IncludeSettings = includeSettings
            };
        }

        private void Given_AuthenticationService()
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

        private void Given_AuthenticationService_ThatReturnsError()
        {
            _authenticationService
                .Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>()))
                .Returns<AuthenticateRequest>(req =>
                {
                    var response = new AuthenticateResponse(req);
                    response.AddError(new HqvException("This is an error"));
                    return Task.FromResult(response);
                });
        }

        private void Given_Thermostats()
        {
            _thermostatProvider
                .Setup(x => x.GetThermostats(It.IsAny<GetThermostatsRequest>()))
                .ReturnsAsync(
                    new[]
                    {
                        Given_AThermostat(),
                    }
                );
        }

        private Domain.Entities.Thermostat Given_AThermostat()
        {
            _thermostatToMock = new Domain.Entities.Thermostat(
                id: _faker.Random.AlphaNumeric(5),
                name: _faker.Random.Word(),
                brand: _faker.Random.Word(),
                model: _faker.Random.Word())
            {
                Settings = new ThermostatSettings(
                    hvacMode: _faker.Random.Word(),
                    desiredHeat: 500,
                    desiredCool: 800,
                    heatRangeHigh: 510,
                    heatRangeLow: 490,
                    coolRangeHigh: 810,
                    coolRangeLow: 790,
                    heatCoolMinDelta: 0),
                Reading = new ThermostatReading(
                    dateTime: DateTime.UtcNow,
                    temperatureInF: 750,
                    humidity: 50),
                Scenes = new ThermostatScene[]
                {
                    new ThermostatScene(
                        type: "template",
                        name: "_Default_",
                        running: true,
                        coolHoldTemp: 870,
                        heatHoldTemp: 450)
                }
            };
            return _thermostatToMock;
        }

        private void When_HandleIsCalled()
        {
            var response = _handler.Handle(_message).Result.ToList();
            _thermostatFromResponse = response.ElementAt(0);
        }

        private void Then_ThermostatInformation_IsCorrect()
        {
            _thermostatFromResponse.Name.Should().Be(_thermostatToMock.Name);
            _thermostatFromResponse.CorrelationId.Should().Be(_message.CorrelationId);
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

            builder.RegisterType<GetThermostatHandler>();

            return builder.Build();
        }
    }
}
