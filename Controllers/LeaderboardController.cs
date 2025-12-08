using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Leaderboard;

namespace nomad_gis_V2.Controllers;

/// <summary>
/// API контроллер для рейтингов и лидербордов.
/// Предоставляет endpoints для получения ТОП-10 пользователей по различным критериям (опыт, открытые точки, ачивки).
/// Доступно для всех пользователей.
/// </summary>
[ApiController]
[Route("api/v1/leaderboard")]
[AllowAnonymous]
public class LeaderboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса LeaderboardController.
    /// </summary>
    /// <param name="context">Контекст базы данных приложения</param>
    public LeaderboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить ТОП-10 пользователей по набранному опыту.
    /// Сортирует пользователей по количеству опыта в убывающем порядке.
    /// </summary>
    /// <returns>Список ТОП-10 пользователей с их позицией в рейтинге, именем, уровнем и опытом</returns>
    ///<summary>
    /// Рейтинг по опыту
    /// </summary>
    [HttpGet("experience")]
    [ProducesResponseType(typeof(List<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExperienceLeaderboard()
    {
        // 1. Сначала получаем данные из БД
        var users = await _context.Users
            .OrderByDescending(u => u.Experience)
            .Take(10)
            .ToListAsync(); // <-- Выполняем запрос к БД

        // 2. Теперь, в памяти, добавляем ранг
        var leaderboard = users.Select((u, index) => new LeaderboardEntryDto
        {
            Rank = index + 1,
            UserId = u.Id,
            AvatarUrl = u.AvatarUrl,
            Username = u.Username,
            Level = u.Level,
            Score = u.Experience
        }).ToList(); // <-- Используем .ToList() т.к. 'users' уже в памяти

        return Ok(leaderboard);
    }

    /// <summary>
    /// Получить ТОП-10 пользователей по количеству открытых точек на карте.
    /// Сортирует пользователей по количеству разблокированных точек в убывающем порядке.
    /// </summary>
    /// <returns>Список ТОП-10 пользователей с их позицией в рейтинге, именем, уровнем и количеством открытых точек</returns>
    [HttpGet("points")]
    [ProducesResponseType(typeof(List<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPointsLeaderboard()
    {
        // 1. Сначала получаем данные из БД
        var sortedData = await _context.UserMapProgress
            .GroupBy(p => p.UserId) // Группируем по ID пользователя
            .Select(g => new
            {
                UserId = g.Key,
                Score = g.Count() // Считаем кол-во записей (открытых точек)
            })
            .OrderByDescending(x => x.Score)
            .Take(10)
            .Join(_context.Users, // Присоединяем инфо о пользователе
                entry => entry.UserId,
                user => user.Id,
                (entry, user) => new { entry, user })
            .ToListAsync(); // <-- Выполняем запрос к БД

        // 2. Теперь, в памяти, добавляем ранг
        var leaderboard = sortedData.Select((data, index) => new LeaderboardEntryDto
        {
            Rank = index + 1,
            UserId = data.user.Id,
            AvatarUrl = data.user.AvatarUrl,
            Username = data.user.Username,
            Level = data.user.Level,
            Score = data.entry.Score
        })
            .ToList(); // <-- Используем .ToList()

        return Ok(leaderboard);
    }

    /// <summary>
    /// Получить ТОП-10 пользователей по количеству полученных ачивок.
    /// Сортирует пользователей по количеству завершенных ачивок в убывающем порядке.
    /// </summary>
    /// <returns>Список ТОП-10 пользователей с их позицией в рейтинге, именем, уровнем и количеством ачивок</returns>
    [HttpGet("achievements")]
    [ProducesResponseType(typeof(List<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAchievementsLeaderboard()
    {
        // 1. Сначала получаем данные из БД
        var sortedData = await _context.UserAchievements
            .Where(ua => ua.IsCompleted) // Считаем только завершенные
            .GroupBy(ua => ua.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Score = g.Count() // Считаем кол-во ачивок
            })
            .OrderByDescending(x => x.Score)
            .Take(10)
            .Join(_context.Users,
                entry => entry.UserId,
                user => user.Id,
                (entry, user) => new { entry, user })
            .ToListAsync(); // <-- Выполняем запрос к БД

        // 2. Теперь, в памяти, добавляем ранг
        var leaderboard = sortedData.Select((data, index) => new LeaderboardEntryDto
        {
            Rank = index + 1,
            UserId = data.user.Id,
            AvatarUrl = data.user.AvatarUrl,
            Username = data.user.Username,
            Level = data.user.Level,
            Score = data.entry.Score
        })
            .ToList(); // <-- Используем .ToList()

        return Ok(leaderboard);
    }
}