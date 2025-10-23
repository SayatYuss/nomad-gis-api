using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

public class AchievementService : IAchievementService
{
    private readonly ApplicationDbContext _context;

    public AchievementService(ApplicationDbContext context)
    {
        _context = context;
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
                RewardPoints = a.RewardPoints
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
            RewardPoints = a.RewardPoints
        };
    }

    public async Task<AchievementResponse> CreateAsync(AchievementCreateRequest request)
    {
        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            RewardPoints = request.RewardPoints
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync();

        return new AchievementResponse
        {
            Id = achievement.Id,
            Code = achievement.Code,
            Title = achievement.Title,
            Description = achievement.Description,
            RewardPoints = achievement.RewardPoints
        };
    }

    public async Task<AchievementResponse> UpdateAsync(Guid id, AchievementUpdateRequest request)
    {
        var achievement = await _context.Achievements.FindAsync(id);
        if (achievement == null) throw new Exception("Achievement not found");

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
            RewardPoints = achievement.RewardPoints
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var achievement = await _context.Achievements.FindAsync(id);
        if (achievement == null) return false;

        _context.Achievements.Remove(achievement);
        await _context.SaveChangesAsync();
        return true;
    }
}
