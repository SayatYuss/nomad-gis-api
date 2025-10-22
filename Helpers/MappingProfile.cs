using AutoMapper;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Из Entity (БД) в DTO (Ответ)
            CreateMap<MapPoint, MapPointRequest>();

            // Из DTO (Запрос) в Entity (БД)
            CreateMap<MapPointCreateRequest, MapPoint>();
            CreateMap<MapPointUpdateRequest, MapPoint>();
        }
    }
}