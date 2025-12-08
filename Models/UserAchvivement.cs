namespace nomad_gis_V2.Models;

/// <summary>
/// Модель, связывающая пользователя с достижением (ачивкой).
/// Отслеживает прогресс пользователя по получению ачивки.
/// </summary>
public class UserAchievement
{
    /// <summary>
    /// ID пользователя.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь (навигационное свойство).
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// ID ачивки.
    /// </summary>
    public Guid AchievementId { get; set; } = Guid.Empty;

    /// <summary>
    /// Ачивка (навигационное свойство).
    /// </summary>
    public virtual Achievement Achievement { get; set; } = null!;

    /// <summary>
    /// Текущий прогресс пользователя по выполнению условий ачивки.
    /// </summary>
    public int ProgressValue { get; set; } = 0;

    /// <summary>
    /// Флаг, указывающий, завершена ли ачивка.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Дата и время завершения ачивки.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
