using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.DTOs.Achievements;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("/me")]
    public async Task<ActionResult<UserDto>> UserProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new Exception("error");

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Experience = user.Experience,
            Level = user.Level
        });
    }

    [HttpGet("my-points")]
    public async Task<ActionResult<List<MapPointRequest>>> UserPoints()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var progress = await _context.UserMapProgress
                .Where(p => p.UserId == userId)
                .ToListAsync();

        return Ok(progress);
    }

    [HttpGet("/my-achievements")]
    public async Task<ActionResult<List<AchievementResponse>>> UserAchievemnts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new Exception("error");

        return Ok(user.UserAchievements);
    }
}