using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Interfaces;

namespace nomad_gis_V2.Controllers;

/// <summary>
/// API контроллер для управления ачивками.
/// Предоставляет endpoints для получения информации об ачивках и администраторских операций (создание, обновление, удаление).
/// </summary>
[ApiController]
[Route("api/v1/achievements")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _service;

    /// <summary>
    /// Инициализирует новый экземпляр класса AchievementsController.
    /// </summary>
    /// <param name="service">Сервис управления ачивками</param>
    public AchievementsController(IAchievementService service)
    {
        _service = service;
    }

    /// <summary>
    /// Получить список всех ачивок.
    /// Доступно для всех пользователей (в том числе неавторизованных).
    /// </summary>
    /// <returns>Список всех доступных ачивок</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AchievementResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AchievementResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>
    /// Получить информацию об ачивке по ID.
    /// Доступно для всех пользователей (в том числе неавторизованных).
    /// </summary>
    /// <param name="id">ID ачивки</param>
    /// <returns>Информация об ачивке (название, описание, иконка, условия и т.д.)</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AchievementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AchievementResponse>> Get(Guid id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    /// <summary>
    /// Создать новую ачивку (только для администраторов).
    /// </summary>
    /// <param name="request">Данные новой ачивки</param>
    /// <returns>Созданная ачивка с ID</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AchievementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AchievementResponse>> Create([FromForm] AchievementCreateRequest request)
    {
        var achievement = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = achievement.Id }, achievement);
    }

    /// <summary>
    /// Обновить ачивку (только для администраторов).
    /// </summary>
    /// <param name="id">ID ачивки для обновления</param>
    /// <param name="request">Новые данные ачивки</param>
    /// <returns>Обновленная ачивка</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AchievementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AchievementResponse>> Update(Guid id, [FromForm] AchievementUpdateRequest request)
    {
        return Ok(await _service.UpdateAsync(id, request));
    }

    /// <summary>
    /// Удалить ачивку (только для администраторов).
    /// </summary>
    /// <param name="id">ID ачивки для удаления</param>
    /// <returns>Сообщение об успешном удалении</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}