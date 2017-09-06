using AutoMapper;
using Hqv.Thermostat.Api.Domain;
using Hqv.Thermostat.Api.Domain.Dtos;
using Hqv.Thermostat.Api.Domain.Entities;
using Hqv.Thermostat.Api.Models;

namespace Hqv.Thermostat.Api
{
    public class Mapper : CSharp.Common.Map.IMapper
    {
        private readonly MapperConfiguration _mapperConfiguration;
        private readonly IMapper _mapper;

        public Mapper()
        {
            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SceneToAddModel, Scene>()
                    .ConstructUsing(s => new Scene(s.HeatHoldTemp, s.ColdHoldTemp));

                cfg.CreateMap<ThermostatToGetModel, GetThermostatsRequest>()
                    .ConstructUsing(s =>
                        new GetThermostatsRequest(s.IncludeReadings, s.IncludeSettings, s.IncludeScenes));

                cfg.CreateMap<ThermostatReading, ThermostatReadingModel>();
                cfg.CreateMap<ThermostatSettings, ThermostatSettingsModel>();
                cfg.CreateMap<ThermostatScene, ThermostatSceneModel>();
                cfg.CreateMap<Domain.Entities.Thermostat, ThermostatModel>()
                    .ForMember(d=>d.CorrelationId, o=>o.Ignore());
            });
            _mapper = _mapperConfiguration.CreateMapper();
        }

        public TDestination Map<TDestination>(object source)
        {
            return _mapper.Map<TDestination>(source);
        }

        public void Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            _mapper.Map(source, destination);
        }

        public void AssertConfigurationIsValid()
        {
            _mapperConfiguration.AssertConfigurationIsValid();
        }
    }
}