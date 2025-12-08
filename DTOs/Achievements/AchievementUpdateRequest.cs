namespace nomad_gis_V2.DTOs.Achievements;

/// <summary>
/// DTO запроса на обновление информации об ачивке (только для администраторов).
/// Позволяет обновлять отдельные поля ачивки.
/// </summary>
public class AchievementUpdateRequest
{
    /// <summary>
    /// Новое название ачивки.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Новое описание условий получения ачивки.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Новое количество опыта, получаемого при разблокировке ачивки.
    /// </summary>
    public int? RewardPoints { get; set; }

    /// <summary>
    /// Новый файл изображения значка ачивки для загрузки.
    /// </summary>
    public IFormFile? BadgeFile { get; set; }
}
