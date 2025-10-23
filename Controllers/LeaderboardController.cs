using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Leaderboard;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/leaderboard")]
[AllowAnonymous]
public class LeaderboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LeaderboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    ///<summary>
    /// Рейтинг по опыту
    /// </summary
    [HttpGet("experience")]
    public async Task<IActionResult> GetExperienceLeaderboard()
    {
        var leaderboard = await _context.Users
            .OrderByDescending(u => u.Experience)
            .Take(10)
            .Select((u, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = u.Id,
                Username = u.Username,
                Level = u.Level,
                Score = u.Experience
            }).ToListAsync();

        return Ok(leaderboard);
    }

    /// <summary>
    /// Рейтинг по Открытым Точкам
    /// </summary>
    [HttpGet("points")]
    public async Task<IActionResult> GetPointsLeaderboard()
    {
        var leaderboard = await _context.UserMapProgress
            .GroupBy(p => p.UserId) // Группируем по ID пользователя
            .Select(g => new 
            {
                UserId = g.Key,
                Score = g.Count() // Считаем кол-во записей (открытых точек)
            })
            .OrderByDescending(x => x.Score)
            .Take(50)
            .Join(_context.Users, // Присоединяем инфо о пользователе
                entry => entry.UserId,
                user => user.Id,
                (entry, user) => new { entry, user })
            .Select((data, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = data.user.Id,
                Username = data.user.Username,
                Level = data.user.Level,
                Score = data.entry.Score
            })
            .ToListAsync();

        return Ok(leaderboard);
    }
    
    /// <summary>
    /// Рейтинг по Полученным Ачивкам
    /// </summary>
    [HttpGet("achievements")]
    public async Task<IActionResult> GetAchievementsLeaderboard()
    {
        var leaderboard = await _context.UserAchievements
            .Where(ua => ua.IsCompleted) // Считаем только завершенные
            .GroupBy(ua => ua.UserId)
            .Select(g => new 
            {
                UserId = g.Key,
                Score = g.Count() // Считаем кол-во ачивок
            })
            .OrderByDescending(x => x.Score)
            .Take(50)
            .Join(_context.Users, 
                entry => entry.UserId,
                user => user.Id,
                (entry, user) => new { entry, user })
            .Select((data, index) => new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = data.user.Id,
                Username = data.user.Username,
                Level = data.user.Level,
                Score = data.entry.Score
            })
            .ToListAsync();

        return Ok(leaderboard);
    }
}