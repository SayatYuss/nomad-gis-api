using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.DTOs.Achievements;
using Microsoft.AspNetCore.Identity;
using nomad_gis_V2.Models;
using nomad_gis_V2.Profile;
using nomad_gis_V2.Exceptions;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<ProfileController> _logger;
    public ProfileController(ApplicationDbContext context, IWebHostEnvironment env, IPasswordHasher<User> passwordHasher, ILogger<ProfileController> logger)
    {
        _context = context;
        _env = env;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> UserProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
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
            return NotFound(new {message = "User not found."});

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

        var avatarsPath = Path.Combine(_env.WebRootPath, "avatars");
        if (!Directory.Exists(avatarsPath))
        {
            Directory.CreateDirectory(avatarsPath);
        }

        string[] existingFiles = Directory.GetFiles(avatarsPath, $"{user.Id}.*");
            
        foreach (var fileToDelete in existingFiles)
        {
            try
            {
                System.IO.File.Delete(fileToDelete);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, $"Could not delete old avatar: {fileToDelete}");
            }
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{user.Id}{fileExtension}";
        var filePath = Path.Combine(avatarsPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var request = HttpContext.Request;
        var publicUrl = $"{request.Scheme}://{request.Host}/avatars/{uniqueFileName}";

        user.AvatarUrl = publicUrl;
        await _context.SaveChangesAsync();

        return Ok(new { avatarUrl = publicUrl });
    }

    [HttpPut("me")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        bool hasChanges = false;

        if (!string.IsNullOrEmpty(request.Username) && user.Username != request.Username)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new DuplicateException("This username is already taken.");
            }
            user.Username = request.Username;
            hasChanges = true;
        }

        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                throw new ValidationException("Current password is required to set new password.");
            }

            var passResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (passResult == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedException("Invalid current password.");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            hasChanges = true;
        }

        if (request.AvatarFile != null && request.AvatarFile.Length > 0)
        {
            var avatarsPath = Path.Combine(_env.WebRootPath, "avatars");
            if (!Directory.Exists(avatarsPath))
            {
                Directory.CreateDirectory(avatarsPath);
            }

            string[] existingFiles = Directory.GetFiles(avatarsPath, $"{user.Id}.*");
            
            foreach (var fileToDelete in existingFiles)
            {
                try
                {
                    System.IO.File.Delete(fileToDelete);
                }
                catch (IOException ex)
                {
                    _logger.LogWarning(ex, $"Could not delete old avatar: {fileToDelete}");
                }
            }

            var fileExtension = Path.GetExtension(request.AvatarFile.FileName);
            var uniqueFileName = $"{user.Id}{fileExtension}";
            var filePath = Path.Combine(avatarsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.AvatarFile.CopyToAsync(stream);
            }

            var httpReq = HttpContext.Request;
            var publicUrl = $"{httpReq.Scheme}://{httpReq.Host}/avatars/{uniqueFileName}";

            user.AvatarUrl = publicUrl;
            hasChanges = true;
        }
        
        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Experience = user.Experience,
            Level = user.Level,
            AvatarUrl = user.AvatarUrl
        });
    }
}