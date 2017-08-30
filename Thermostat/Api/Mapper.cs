using AutoMapper;
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
                cfg.CreateMap<SceneToAddModel, Scene>();
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