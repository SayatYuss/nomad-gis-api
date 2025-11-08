using AutoMapper;
using NetTopologySuite;
using NetTopologySuite.Geometries; // <-- Добавьте
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Helpers
{
    public class MappingProfile : AutoMapper.Profile
    {
        // Фабрика для создания Point с SRID 4326
        private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        
        public MappingProfile()
        {
            // Из Entity (БД) в DTO (Ответ)
            CreateMap<MapPoint, MapPointRequest>()
                // (X = Longitude, Y = Latitude)
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Y))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.X));

            // Из DTO (Запрос) в Entity (БД)
            CreateMap<MapPointCreateRequest, MapPoint>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                    _geometryFactory.CreatePoint(new Coordinate(src.Longitude, src.Latitude))
                ));
            
            CreateMap<MapPointUpdateRequest, MapPoint>()
                 .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                    _geometryFactory.CreatePoint(new Coordinate(src.Longitude, src.Latitude))
                ));
                
            CreateMap<Achievement, AchievementResponse>();
        }
    }
}