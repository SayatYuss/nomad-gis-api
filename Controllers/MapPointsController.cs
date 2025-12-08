using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Interfaces;

namespace nomad_gis_V2.Controllers
{
    /// <summary>
    /// API контроллер для управления точками на карте.
    /// Предоставляет endpoints для получения, создания, обновления и удаления точек интереса.
    /// Создание/обновление/удаление доступно только администраторам.
    /// </summary>
    [ApiController]
    [Route("api/v1/points")]
    [Authorize]
    public class MapPointsController : ControllerBase
    {
        private readonly IMapPointService _mapPointService;

        /// <summary>
        /// Инициализирует новый экземпляр класса MapPointsController.
        /// </summary>
        /// <param name="mapPointService">Сервис управления точками на карте</param>
        public MapPointsController(IMapPointService mapPointService)
        {
            _mapPointService = mapPointService;
        }

        /// <summary>
        /// Получить список всех точек на карте.
        /// Доступно для всех пользователей (в том числе неавторизованных).
        /// </summary>
        /// <returns>Список всех точек на карте с координатами и информацией</returns>
        // GET: api/MapPoints
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var points = await _mapPointService.GetAllAsync();
            return Ok(points);
        }

        /// <summary>
        /// Получить информацию о конкретной точке на карте по ID.
        /// Доступно для всех пользователей (в том числе неавторизованных).
        /// </summary>
        /// <param name="id">ID точки на карте</param>
        /// <returns>Информация о точке (название, описание, координаты, и т.д.)</returns>
        // GET: api/MapPoints/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var point = await _mapPointService.GetByIdAsync(id);
            if (point == null) return NotFound();
            return Ok(point);
        }

        /// <summary>
        /// Создать новую точку на карте (только для администраторов).
        /// </summary>
        /// <param name="dto">Данные новой точки (название, описание, координаты, и т.д.)</param>
        /// <returns>Созданная точка с ID</returns>
        // POST: api/MapPoints
        [HttpPost]
        [Authorize(Roles = "Admin")] // <-- ДОБАВЛЕНО
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] MapPointCreateRequest dto)
        {
            var created = await _mapPointService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Обновить информацию о точке на карте (только для администраторов).
        /// </summary>
        /// <param name="id">ID точки для обновления</param>
        /// <param name="dto">Новые данные точки</param>
        /// <returns>Обновленная точка</returns>
        // PUT: api/MapPoints/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // <-- ДОБАВЛЕНО
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] MapPointUpdateRequest dto)
        {
            var updated = await _mapPointService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        /// <summary>
        /// Удалить точку с карты (только для администраторов).
        /// </summary>
        /// <param name="id">ID точки для удаления</param>
        /// <returns>Сообщение об успешном удалении</returns>
        // DELETE: api/MapPoints/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // <-- ДОБАВЛЕНО
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _mapPointService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}