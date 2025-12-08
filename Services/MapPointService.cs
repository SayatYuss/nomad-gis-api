using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services
{
    /// <summary>
    /// Сервис для управления точками на карте.
    /// Обрабатывает операции CRUD для точек интереса.
    /// </summary>
    public class MapPointService : IMapPointService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса для работы с точками.
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        /// <param name="mapper">Маппер для преобразования моделей в DTO</param>
        public MapPointService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Получает список всех точек на карте.
        /// </summary>
        /// <returns>Список всех точек с координатами, радиусом разблокировки и описанием</returns>
        public async Task<List<MapPointRequest>> GetAllAsync()
        {
            var points = await _context.MapPoints.ToListAsync();
            return _mapper.Map<List<MapPointRequest>>(points);
        }

        /// <summary>
        /// Получает точку по уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор точки</param>
        /// <returns>Данные точки или null если точка не найдена</returns>
        public async Task<MapPointRequest?> GetByIdAsync(Guid id)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return null;

            return _mapper.Map<MapPointRequest>(mp);
        }

        /// <summary>
        /// Создаёт новую точку на карте.
        /// </summary>
        /// <param name="dto">Данные для создания новой точки (название, координаты, радиус, описание)</param>
        /// <returns>Созданная точка с присвоенным ID</returns>
        public async Task<MapPointRequest> CreateAsync(MapPointCreateRequest dto)
        {
            var mp = _mapper.Map<MapPoint>(dto);

            mp.Id = Guid.NewGuid();
            mp.CreatedAt = DateTime.UtcNow;

            _context.MapPoints.Add(mp);
            await _context.SaveChangesAsync();

            return _mapper.Map<MapPointRequest>(mp);
        }

        /// <summary>
        /// Обновляет существующую точку на карте.
        /// </summary>
        /// <param name="id">Уникальный идентификатор точки для обновления</param>
        /// <param name="dto">Новые данные для точки</param>
        /// <returns>Обновленная точка или null если точка не найдена</returns>
        public async Task<MapPointRequest?> UpdateAsync(Guid id, MapPointUpdateRequest dto)
        {
            var mp = await _context.MapPoints.FindAsync(id);
            if (mp == null) return null;

            _mapper.Map(dto, mp);

            _context.MapPoints.Update(mp);
            await _context.SaveChangesAsync();

            return _mapper.Map<MapPointRequest>(mp);
        }

        /// <summary>
        /// Удаляет точку с карты по идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор точки для удаления</param>
        /// <returns>True если точка успешно удалена, false если точка не найдена</returns>
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
