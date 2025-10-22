using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services
{
    public class MapPointService : IMapPointService
    {
        private readonly ApplicationDbContext _context;

        public MapPointService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MapPointRequest>> GetAllAsync()
        {
            return await _context.MapPoints
                .Select(mp => new MapPointRequest
                {
                    Id = mp.Id,
                    Name = mp.Name,
                    Latitude = mp.Latitude,
                    Longitude = mp.Longitude,
                    UnlockRadiusMeters = mp.UnlockRadiusMeters,
                    Description = mp.Description,
                    CreatedAt = mp.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<MapPointRequest?> GetByIdAsync(Guid id)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return null;

            return new MapPointRequest
            {
                Id = mp.Id,
                Name = mp.Name,
                Latitude = mp.Latitude,
                Longitude = mp.Longitude,
                UnlockRadiusMeters = mp.UnlockRadiusMeters,
                Description = mp.Description,
                CreatedAt = mp.CreatedAt
            };
        }

        public async Task<MapPointRequest> CreateAsync(MapPointCreateRequest dto)
        {
            var mp = new MapPoint
            {
                Name = dto.Name,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                UnlockRadiusMeters = dto.UnlockRadiusMeters,
                Description = dto.Description
            };

            _context.MapPoints.Add(mp);
            await _context.SaveChangesAsync();

            return new MapPointRequest
            {
                Id = mp.Id,
                Name = mp.Name,
                Latitude = mp.Latitude,
                Longitude = mp.Longitude,
                UnlockRadiusMeters = mp.UnlockRadiusMeters,
                Description = mp.Description,
                CreatedAt = mp.CreatedAt
            };
        }

        public async Task<MapPointRequest?> UpdateAsync(Guid id, MapPointUpdateRequest dto)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return null;

            mp.Name = dto.Name;
            mp.Latitude = dto.Latitude;
            mp.Longitude = dto.Longitude;
            mp.UnlockRadiusMeters = dto.UnlockRadiusMeters;
            mp.Description = dto.Description;

            _context.MapPoints.Update(mp);
            await _context.SaveChangesAsync();

            return new MapPointRequest
            {
                Id = mp.Id,
                Name = mp.Name,
                Latitude = mp.Latitude,
                Longitude = mp.Longitude,
                UnlockRadiusMeters = mp.UnlockRadiusMeters,
                Description = mp.Description,
                CreatedAt = mp.CreatedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return false;

            _context.MapPoints.Remove(mp);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
