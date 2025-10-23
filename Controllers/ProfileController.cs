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
    private readonly IWebHostEnvironment _env;
    public ProfileController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("me")]
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

    [HttpGet("my-achievements")]
    public async Task<ActionResult<List<AchievementResponse>>> UserAchievemnts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new Exception("error");

        return Ok(user.UserAchievements);
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(5_000_000)] // Ограничение 5MB
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // 1. Создаем путь к папке (wwwroot/avatars)
        // _env.WebRootPath указывает на папку 'wwwroot'
        var avatarsPath = Path.Combine(_env.WebRootPath, "avatars");
        if (!Directory.Exists(avatarsPath))
        {
            Directory.CreateDirectory(avatarsPath);
        }

        // 2. Генерируем уникальное имя файла
        // (Например: 123e4567-e89b-12d3-a456-426614174000.jpg)
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{user.Id}{fileExtension}";
        var filePath = Path.Combine(avatarsPath, uniqueFileName);

        // 3. Сохраняем файл на диск
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 4. Генерируем публичный URL
        // (Ваш API должен быть доступен по http/https)
        var request = HttpContext.Request;
        var publicUrl = $"{request.Scheme}://{request.Host}/avatars/{uniqueFileName}";

        // 5. Сохраняем URL в базу
        user.AvatarUrl = publicUrl;
        await _context.SaveChangesAsync();

        return Ok(new { avatarUrl = publicUrl });
    }
}