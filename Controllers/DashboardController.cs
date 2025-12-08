using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using System.Threading.Tasks; // Убедитесь, что это добавлено
using System; // Убедитесь, что это добавлено

namespace nomad_gis_V2.Controllers;

/// <summary>
/// API контроллер для админ-панели и статистики.
/// Предоставляет endpoints для получения общей статистики по приложению (только для администраторов).
/// </summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Roles = "Admin")] // Только для админов
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса DashboardController.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения</param>
    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить статистику приложения.
    /// Возвращает общую информацию: количество пользователей, точек, сообщений, открытых точек и ачивок.
    /// Включает статистику за последний день.
    /// </summary>
    /// <returns>Объект со статистикой приложения</returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalPoints = await _context.MapPoints.CountAsync();
        var totalMessages = await _context.Messages.CountAsync();
        var totalUnlocks = await _context.UserMapProgress.CountAsync();
        var totalAchievementsWon = await _context.UserAchievements.CountAsync(ua => ua.IsCompleted);

        var newUsersToday = await _context.Users
            .CountAsync(u => u.CreatedAt > DateTime.UtcNow.AddDays(-1));

        var newMessagesToday = await _context.Messages
            .CountAsync(m => m.CreatedAt > DateTime.UtcNow.AddDays(-1));

        var stats = new
        {
            TotalUsers = totalUsers,
            TotalMapPoints = totalPoints,
            TotalMessages = totalMessages,
            TotalUnlocks = totalUnlocks,
            TotalAchievementsWon = totalAchievementsWon,
            NewUsersToday = newUsersToday,
            NewMessagesToday = newMessagesToday
        };

        return Ok(stats);
    }
}
