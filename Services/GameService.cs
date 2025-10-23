using AutoMapper;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _context;
    private readonly IAchievementService _achievementService;
    private readonly IMapper _mapper;
    public GameService(ApplicationDbContext context, IAchievementService achievementService, IMapper mapper)
    {
        _context = context;
        _achievementService = achievementService;
        _mapper = mapper;
    }
    public async Task<UnlockResponse> CheckAndUnlockPointsAsync(Guid userId, CheckLocationRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var unlockedPointIds = await _context.UserMapProgress
            .Where(p => p.UserId == userId)
            .Select(p => p.MapPointId)
            .ToHashSetAsync();

        var potentialPointsToUnlock = await _context.MapPoints
            .Where(p => !unlockedPointIds.Contains(p.Id)) 
            .ToListAsync();

        foreach (var point in potentialPointsToUnlock)
        {
            var distance = CalculateDistance(request.Latitude, request.Longitude, point.Latitude, point.Longitude);
            System.Console.WriteLine($"Distance to point {point.Name}: {distance} meters");
            if (distance <= point.UnlockRadiusMeters)
            {
                var progress = new UserMapProgress
                {
                    UserId = userId,
                    MapPointId = point.Id,
                    UnlockedAt = DateTime.UtcNow
                };
                _context.UserMapProgress.Add(progress);

                int expGained = 100; // Допустим, 100 очков за точку
                user.Experience += expGained;

                var newAchievemnts = await _achievementService.CheckUnlockAchievementsAsync(user, point);

                int achievementsExp = newAchievemnts.Sum(a => a.RewardPoints);
                int totalExpGained = expGained + achievementsExp;

                // 4. Сохраняем и выходим
                await _context.SaveChangesAsync();

                return new UnlockResponse
                {
                    Success = true,
                    Message = $"Вы открыли точку: {point.Name}!",
                    UnlockedPointId = point.Id,
                    ExperienceGained = totalExpGained,
                    unblockedAchievemnts = _mapper.Map<List<AchievementResponse>>(newAchievemnts)
                };
            }
        }

        return new UnlockResponse { Success = false, Message = "Поблизости нет новых точек." };
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371e3;

        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = R * c;
        return distance;
    }  

    private static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}