namespace nomad_gis_V2.DTOs.Achievements;

/// <summary>
/// DTO ответа с информацией об ачивке.
/// Используется для передачи данных об ачивке клиенту.
/// </summary>
public class AchievementResponse
{
    /// <summary>
    /// Уникальный ID ачивки.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Код ачивки для внутреннего использования.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Название ачивки.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание условий получения ачивки.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Количество опыта, получаемого при разблокировке ачивки.
    /// </summary>
    public int RewardPoints { get; set; }

    /// <summary>
    /// URL изображения значка ачивки.
    /// </summary>
    public string? BadgeImageUrl { get; set; }
}
