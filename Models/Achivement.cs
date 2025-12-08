using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель достижения (ачивки) в приложении.
/// Содержит информацию о условиях получения ачивки и связанных пользователях.
/// </summary>
public class Achievement
{
    /// <summary>
    /// Уникальный идентификатор ачивки.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Код ачивки для внутреннего использования (например, "OPEN_10_POINTS").
    /// </summary>
    [Required, StringLength(100)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Название ачивки, отображаемое пользователю.
    /// </summary>
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание условий получения ачивки.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Целевое значение для получения ачивки (например, 10 открытых точек).
    /// </summary>
    public int GoalValue { get; set; } = 0;

    /// <summary>
    /// Количество опыта, получаемого при разблокировке ачивки.
    /// </summary>
    public int RewardPoints { get; set; } = 0;

    /// <summary>
    /// URL изображения значка ачивки.
    /// </summary>
    [StringLength(200)]
    public string? BadgeImageUrl { get; set; }

    /// <summary>
    /// Пользователи, получившие эту ачивку.
    /// </summary>
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}