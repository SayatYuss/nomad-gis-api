namespace nomad_gis_V2.DTOs.Achievements;

/// <summary>
/// DTO запроса на создание новой ачивки (только для администраторов).
/// Содержит все необходимые данные для создания ачивки.
/// </summary>
public class AchievementCreateRequest
{
    /// <summary>
    /// Код ачивки для внутреннего использования (например, "OPEN_10_POINTS").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Название ачивки, отображаемое пользователю.
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
    /// Файл изображения значка ачивки для загрузки.
    /// </summary>
    public IFormFile? BadgeFile { get; set; }
}
