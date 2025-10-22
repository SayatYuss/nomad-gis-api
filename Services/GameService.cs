using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

public class GameService : IGameService
{
    private readonly ApplicationDbContext _context;
    public GameService(ApplicationDbContext context)
    {
        _context = context;
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

        // 3. Перебираем только 950 точек
        foreach (var point in potentialPointsToUnlock)
        {
            // 5b. Считаем расстояние
            var distance = CalculateDistance(request.Latitude, request.Longitude, point.Latitude, point.Longitude);
            System.Console.WriteLine($"Distance to point {point.Name}: {distance} meters");
            // 5c. Проверяем радиус
            if (distance <= point.UnlockRadiusMeters)
            {
                // УСПЕХ! Пользователь в зоне

                // 1. Создаем запись о прогрессе
                var progress = new UserMapProgress
                {
                    UserId = userId,
                    MapPointId = point.Id,
                    UnlockedAt = DateTime.UtcNow
                };
                _context.UserMapProgress.Add(progress);

                // 2. Начисляем опыт (п. 4)
                int expGained = 100; // Допустим, 100 очков за точку
                user.Experience += expGained;

                // 3. (Позже) Проверяем ачивки (п. 3)
                // await _achievementService.CheckForUnlockAchievements(user);

                // 4. Сохраняем и выходим
                await _context.SaveChangesAsync();

                return new UnlockResponse
                {
                    Success = true,
                    Message = $"Вы открыли точку: {point.Name}!",
                    UnlockedPointId = point.Id,
                    ExperienceGained = expGained
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