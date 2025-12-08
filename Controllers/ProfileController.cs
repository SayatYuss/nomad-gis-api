using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Profile;
using nomad_gis_V2.Interfaces;

namespace nomad_gis_V2.Controllers;

/// <summary>
/// API контроллер для работы с профилем текущего пользователя.
/// Предоставляет endpoints для получения информации о профиле, истории открытых точек, ачивок и загрузки аватара.
/// </summary>
[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    /// <summary>
    /// Инициализирует новый экземпляр класса ProfileController.
    /// </summary>
    /// <param name="profileService">Сервис управления профилем пользователя</param>
    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Получить информацию о текущем профиле пользователя.
    /// </summary>
    /// <returns>Данные профиля текущего пользователя</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UserProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _profileService.GetUserProfileAsync(userId);

        return Ok(user);
    }

    /// <summary>
    /// Получить список всех открытых точек карты текущим пользователем.
    /// </summary>
    /// <returns>Список открытых точек с информацией о дате открытия</returns>
    [HttpGet("my-points")]
    [ProducesResponseType(typeof(List<MapPointRequest>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<MapPointRequest>>> UserPoints()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _profileService.GetUserPointsAsync(userId);

        return Ok(progress);
    }

    /// <summary>
    /// Получить список всех ачивок пользователя (активированных и не активированных).
    /// </summary>
    /// <returns>Список всех ачивок с информацией о статусе выполнения</returns>
    [HttpGet("my-achievements")]
    [ProducesResponseType(typeof(List<AchievementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AchievementResponse>>> UserAchievemnts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userAchievements = await _profileService.GetUserAchievementsAsync(userId);

        return Ok(userAchievements);
    }

    /// <summary>
    /// Загрузить новый аватар пользователя.
    /// Максимальный размер файла: 5 МБ. Поддерживаемые форматы: jpg, png, gif.
    /// </summary>
    /// <param name="file">Файл изображения аватара</param>
    /// <returns>URL загруженного аватара</returns>
    [HttpPost("avatar")]
    [RequestSizeLimit(5_000_000)] // Ограничение 5MB
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var publicUrl = await _profileService.UploadAvatarAsync(userId, file);

        return Ok(new { avatarUrl = publicUrl });
    }

    /// <summary>
    /// Обновить информацию профиля пользователя.
    /// Может включать аватар и другие данные профиля.
    /// </summary>
    /// <param name="request">Данные для обновления профиля</param>
    /// <returns>Обновленные данные профиля</returns>
    [HttpPut("me")]
    [RequestSizeLimit(5_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedUser = await _profileService.UpdateProfileAsync(userId, request);

        return Ok(updatedUser);
    }
}