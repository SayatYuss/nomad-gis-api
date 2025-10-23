using nomad_gis_V2.DTOs.Achievements;

namespace nomad_gis_V2.DTOs.Game;
public class UnlockResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public Guid? UnlockedPointId { get; set; }
    public int ExperienceGained { get; set; } = 0;
    public List<AchievementResponse> unlockedAchievemnts { get; set; } = new();
}
