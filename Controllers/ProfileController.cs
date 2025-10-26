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
using Amazon.S3;
using Amazon.S3.Model;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<ProfileController> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _config;


    // Обновите конструктор
    public ProfileController(ApplicationDbContext context, 
                             IPasswordHasher<User> passwordHasher, 
                             ILogger<ProfileController> logger,
                             IAmazonS3 s3Client,
                             IConfiguration config)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _s3Client = s3Client; 
        _config = config; 
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

        var bucketName = _config["R2Storage:BucketName"];
        var publicUrlBase = _config["R2Storage:PublicUrlBase"];
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{user.Id}{fileExtension}";

        try
        {
            // 1. Если у юзера уже есть аватар, удаляем старый из R2
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                try
                {
                    var oldKey = Path.GetFileName(new Uri(user.AvatarUrl).LocalPath);
                    await _s3Client.DeleteObjectAsync(bucketName, oldKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete old avatar from R2: {OldUrl}", user.AvatarUrl);
                }
            }

            await using var stream = file.OpenReadStream();
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = uniqueFileName,
                InputStream = stream,
                ContentType = file.ContentType,
                
                // --- РЕШЕНИЕ ИЗ ДОКУМЕНТАЦИИ CLOUDFLARE ---
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
                // --- КОНЕЦ ---
            };

            var response = await _s3Client.PutObjectAsync(putRequest);

            var publicUrl = $"{publicUrlBase?.TrimEnd('/')}/{uniqueFileName}";

            user.AvatarUrl = publicUrl;
            await _context.SaveChangesAsync();

            return Ok(new { avatarUrl = publicUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar to R2 for user {UserId}", userId);
            return StatusCode(500, new { message = "Error uploading file." });
        }
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
            // --- ЛОГИКА R2 ---
            var bucketName = _config["R2Storage:BucketName"];
            var publicUrlBase = _config["R2Storage:PublicUrlBase"];
            var fileExtension = Path.GetExtension(request.AvatarFile.FileName);
            var uniqueFileName = $"{user.Id}{fileExtension}"; // Key

            try
            {
                // 1. Удаляем старый файл, если он был
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    try
                    {
                        var oldKey = Path.GetFileName(new Uri(user.AvatarUrl).LocalPath);
                        await _s3Client.DeleteObjectAsync(bucketName, oldKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not delete old avatar from R2: {OldUrl}", user.AvatarUrl);
                    }
                }

                // 2. Загружаем новый
                await using var stream = request.AvatarFile.OpenReadStream();
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = uniqueFileName,
                    InputStream = stream,
                    ContentType = request.AvatarFile.ContentType,
                    
                    // --- РЕШЕНИЕ ИЗ ДОКУМЕНТАЦИИ CLOUDFLARE ---
                    DisablePayloadSigning = true,
                    DisableDefaultChecksumValidation = true
                    // --- КОНЕЦ ---
                };

                await _s3Client.PutObjectAsync(putRequest);

                // 3. Формируем URL
                var publicUrl = $"{publicUrlBase?.TrimEnd('/')}/{uniqueFileName}";
                user.AvatarUrl = publicUrl;
                hasChanges = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar to R2 during profile update for user {UserId}", userId);
                // Вы можете пробросить ошибку или вернуть BadRequest
                throw new Exception("Error occurred while uploading new avatar.", ex);
            }
            // --- КОНЕЦ ЛОГИКИ R2 ---
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