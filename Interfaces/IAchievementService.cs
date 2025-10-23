using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Interfaces;

public interface IAchievementService
{
    Task<List<AchievementResponse>> GetAllAsync();
    Task<List<Achievement>> CheckUnlockAchievementsAsync(User user, MapPoint unlockedPoint);
    Task<AchievementResponse> GetByIdAsync(Guid id);
    Task<AchievementResponse> CreateAsync(AchievementCreateRequest request);
    Task<AchievementResponse> UpdateAsync(Guid id, AchievementUpdateRequest request);
    Task<bool> DeleteAsync(Guid id);
}
