using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Helpers;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

/// <summary>
/// Сервис для управления достижениями пользователей.
/// Проверяет условия разблокировки и управляет наградами и опытом.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly ApplicationDbContext _context;
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _config;
    private readonly ILogger<AchievementService> _logger;
    private readonly IExperienceService _experienceService;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса для управления достижениями.
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="s3Client">Клиент AWS S3 для загрузки иконок достижений</param>
    /// <param name="config">Конфигурация приложения</param>
    /// <param name="logger">Логгер для отслеживания ошибок</param>
    /// <param name="experienceService">Сервис для начисления опыта за достижения</param>
    public AchievementService(ApplicationDbContext context,
                              IAmazonS3 s3Client,
                              IConfiguration config,
                              ILogger<AchievementService> logger,
                              IExperienceService experienceService)
    {
        _context = context;
        _s3Client = s3Client;
        _config = config;
        _logger = logger;
        _experienceService = experienceService;
    }

    /// <summary>
    /// Проверяет, какие достижения может разблокировать пользователь в зависимости от события.
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="eventType">Тип события (разблокировка точки, сообщение, лайк и т.д.)</param>
    /// <param name="context">Контекст с информацией о прогрессе пользователя</param>
    /// <returns>Список новых разблокированных достижений</returns>
    public async Task<List<Achievement>> CheckAchievementsAsync(Guid userId, AchievementEvent eventType, AchievementContext context)
    {
        var grantedAchievemnts = new List<Achievement?>();

        switch (eventType)
        {
            case AchievementEvent.PointedUnlocked:
                grantedAchievemnts.AddRange(
                    await HandlePointAchievementsAsync(userId, context)
                );
                break;
            case AchievementEvent.MessagePosted:
                grantedAchievemnts.AddRange(
                    await HandleMessageAchievementsAsync(userId, context)
                );
                break;
            case AchievementEvent.MessageLiked:
                grantedAchievemnts.AddRange(
                    await HandleLikeAchievementsAsync(userId, context)
                );
                break;
            case AchievementEvent.MessageLikeReceived:
                grantedAchievemnts.AddRange(
                    await HandleLikeReceivedAchievementsAsync(userId, context)
                );
                break;
        }

        return grantedAchievemnts.OfType<Achievement>().ToList();
    }

    private async Task<List<Achievement?>> HandlePointAchievementsAsync(Guid userId, AchievementContext context)
    {
        var achievements = new List<Achievement?>();
        int count = context.TotalPointsUnlocked;

        if (count == 1)
        {
            achievements.Add(await TryGrantAchievementAsync(userId, "OPEN_1_POINT"));
        }
        if (count == 5)
        {
            achievements.Add(await TryGrantAchievementAsync(userId, "OPEN_10_POINT"));
        }
        if (count == 10)
        {
            achievements.Add(await TryGrantAchievementAsync(userId, "OPEN_50_POINT"));
        }

        return achievements;
    }

    private async Task<List<Achievement?>> HandleLikeReceivedAchievementsAsync(Guid userId, AchievementContext context)
    {
        var achievements = new List<Achievement?>();
        int count = context.LikesOnThisMessage;


        if (count == 1)
        {
            achievements.Add(await TryGrantAchievementAsync(userId, "MESSAGE_1_LIKE"));
        }
        if (count == 5)
        {
            achievements.Add(await TryGrantAchievementAsync(userId, "MESSAGE_5_LIKES"));
        }
        if (count == 10)
        {
            achievements.Add(await TryGrantAchievementAsync(userId, "MESSAGE_10_LIKES"));
        }

        return achievements;
    }

    private async Task<List<Achievement?>> HandleMessageAchievementsAsync(Guid userId, AchievementContext context)
    {
        if (context.TotalMessagesPosted == 1)
        {
            return new List<Achievement?>
            {
                await TryGrantAchievementAsync(userId, "FIRST_COMMENT")
            };
        }
        return new List<Achievement?>();
    }

    private async Task<List<Achievement?>> HandleLikeAchievementsAsync(Guid userId, AchievementContext context)
    {
        if (context.TotalMessagesLiked == 1)
        {
            return new List<Achievement?>
            {
                await TryGrantAchievementAsync(userId, "FIRST_LIKE")
            };
        }
        return new List<Achievement?>();
    }

    private async Task<Achievement?> TryGrantAchievementAsync(Guid userId, string achievementCode)
    {
        var achievement = await _context.Achievements
                .FirstOrDefaultAsync(a => a.Code == achievementCode);

        if (achievement == null)
        {
            _logger.LogWarning("Achievement with code {Code} not found.", achievementCode);
            return null;
        }

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
            await _experienceService.AddExperienceAsync(user, achievement.RewardPoints);
        }

        return achievement;
    }

    /// <summary>
    /// Получает список всех доступных достижений.
    /// </summary>
    /// <returns>Список всех достижений с описанием и наградами</returns>
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

    /// <summary>
    /// Получает информацию о достижении по его ID.
    /// </summary>
    /// <param name="id">Уникальный идентификатор достижения</param>
    /// <returns>Полная информация о достижении</returns>
    /// <exception cref="Exception">Выбрасывает исключение если достижение не найдено</exception>
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

    /// <summary>
    /// Создаёт новое достижение с иконкой в S3.
    /// </summary>
    /// <param name="request">Данные для создания достижения (код, название, описание, награда, иконка)</param>
    /// <returns>Созданное достижение с присвоенным ID</returns>
    /// <exception cref="BadHttpRequestException">Выбрасывает исключение если достижение с таким кодом уже существует</exception>
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

    /// <summary>
    /// Обновляет информацию существующего достижения, включая иконку.
    /// </summary>
    /// <param name="id">Уникальный идентификатор достижения для обновления</param>
    /// <param name="request">Новые данные для достижения</param>
    /// <returns>Обновленное достижение</returns>
    /// <exception cref="Exception">Выбрасывает исключение если достижение не найдено</exception>
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

    /// <summary>
    /// Удаляет достижение и его иконку из S3.
    /// </summary>
    /// <param name="id">Уникальный идентификатор достижения для удаления</param>
    /// <returns>True если достижение успешно удалено, false если достижение не найдено</returns>
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
