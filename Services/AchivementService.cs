using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

public class AchievementService : IAchievementService
{
    private readonly ApplicationDbContext _context;
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _config;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(ApplicationDbContext context, 
                              IAmazonS3 s3Client, 
                              IConfiguration config, 
                              ILogger<AchievementService> logger)
    {
        _context = context;
        _s3Client = s3Client;
        _config = config;
        _logger = logger;
    }

    public async Task<List<Achievement>> CheckUnlockAchievementsAsync(User user, MapPoint unlockedPoint)
    {
        var grantedAchievements = new List<Achievement>();

        var unlockedCount = await _context.UserMapProgress
                .CountAsync(p => p.UserId == user.Id);

        if (unlockedCount == 1)
        {
            var ach = await GrantAchivementAsync(user.Id, "OPEN_1_POINT");
            if (ach != null) grantedAchievements.Add(ach);
        }

        if (unlockedCount == 5)
        {
            var ach = await GrantAchivementAsync(user.Id, "OPEN_10_POINT");
            if (ach != null) grantedAchievements.Add(ach);
        }

        if (unlockedCount == 10)
        {
            var ach = await GrantAchivementAsync(user.Id, "OPEN_50_POINT");
            if (ach != null) grantedAchievements.Add(ach);
        }

        return grantedAchievements;
    }
    
    private async Task<Achievement?> GrantAchivementAsync(Guid userId, string achievementCode)
    {
        var achievement = await _context.Achievements
                .FirstOrDefaultAsync(a => a.Code == achievementCode);
        if (achievement == null) return null;

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;


        var alredyExists = await _context.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

        if (alredyExists == true) return null;

        var userAchievements = new UserAchievement
        {
            UserId = userId,
            User = user,
            AchievementId = achievement.Id,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        _context.UserAchievements.Add(userAchievements);

        if (achievement.RewardPoints > 0)
        {
            user.Experience += achievement.RewardPoints;
        }

        return achievement;
    }

    public async Task<List<AchievementResponse>> GetAllAsync()
    {
        return await _context.Achievements
            .Select(a => new AchievementResponse
            {
                Id = a.Id,
                Code = a.Code,
                Title = a.Title,
                Description = a.Description,
                RewardPoints = a.RewardPoints,
                BadgeImageUrl = a.BadgeImageUrl
            })
            .ToListAsync();
    }

    public async Task<AchievementResponse> GetByIdAsync(Guid id)
    {
        var a = await _context.Achievements.FindAsync(id);
        if (a == null) throw new Exception("Achievement not found");

        return new AchievementResponse
        {
            Id = a.Id,
            Code = a.Code,
            Title = a.Title,
            Description = a.Description,
            RewardPoints = a.RewardPoints,
            BadgeImageUrl = a.BadgeImageUrl
        };
    }

    public async Task<AchievementResponse> CreateAsync(AchievementCreateRequest request)
    {
        if (await _context.Achievements.AnyAsync(a => a.Code == request.Code))
        {
            throw new BadHttpRequestException("Achievement already exists");
        }

        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            RewardPoints = request.RewardPoints
        };

        // Логика загрузки файла
        if (request.BadgeFile != null)
        {
            var publicUrl = await UploadBadgeAsync(request.BadgeFile, achievement.Code);
            achievement.BadgeImageUrl = publicUrl;
        }

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync();

        return new AchievementResponse
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description,
            RewardPoints = achievement.RewardPoints,
            BadgeImageUrl = achievement.BadgeImageUrl
        };
    }

    public async Task<AchievementResponse> UpdateAsync(Guid id, AchievementUpdateRequest request)
    {
        var achievement = await _context.Achievements.FindAsync(id);
        if (achievement == null) throw new Exception("Achievement not found");

        if (request.BadgeFile != null)
        {
            var publicUrl = await UploadBadgeAsync(request.BadgeFile, achievement.Code, achievement.BadgeImageUrl);
            achievement.BadgeImageUrl = publicUrl;
        }

        if (!string.IsNullOrEmpty(request.Title)) achievement.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Description)) achievement.Description = request.Description;
        if (request.RewardPoints.HasValue) achievement.RewardPoints = request.RewardPoints.Value;

        await _context.SaveChangesAsync();

        return new AchievementResponse
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description,
            RewardPoints = achievement.RewardPoints,
            BadgeImageUrl = achievement.BadgeImageUrl
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var achievement = await _context.Achievements.FindAsync(id);
        if (achievement == null) return false;

        if (!string.IsNullOrEmpty(achievement.BadgeImageUrl))
        {
            try
            {
                var bucketName = _config["R2Storage:BucketName"];
                var oldKey = Path.GetFileName(new Uri(achievement.BadgeImageUrl).LocalPath);
                await _s3Client.DeleteObjectAsync(bucketName, oldKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete old badge from R2: {OldUrl}", achievement.BadgeImageUrl);
            }
        }

        _context.Achievements.Remove(achievement);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> UploadBadgeAsync(IFormFile file, string achievementCode, string? oldImageUrl = null)
    {
        var bucketName = _config["R2Storage:BucketName"];
        var publicUrlBase = _config["R2Storage:PublicUrlBase"];
        
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{achievementCode.ToLower()}{fileExtension}";

        try
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                try
                {
                    var oldKey = Path.GetFileName(new Uri(oldImageUrl).LocalPath);
                    if (oldKey != uniqueFileName)
                    {
                         await _s3Client.DeleteObjectAsync(bucketName, oldKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete old badge from R2: {OldUrl}", oldImageUrl);
                }
            }

            await using var stream = file.OpenReadStream();
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = uniqueFileName,
                InputStream = stream,
                ContentType = file.ContentType,
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            await _s3Client.PutObjectAsync(putRequest);

            return $"{publicUrlBase?.TrimEnd('/')}/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading badge to R2 for achievement {Code}", achievementCode);
            throw new Exception("Error uploading file.", ex);
        }
    }
}
