namespace nomad_gis_V2.DTOs.Achievements;

/// <summary>
/// DTO для контекста достижений пользователя.
/// Содержит информацию о прогрессе для проверки условий активации ачивок.
/// </summary>
public class AchievementContext
{
    /// <summary>
    /// Общее количество открытых точек на карте.
    /// </summary>
    public int TotalPointsUnlocked { get; set; } = 0;

    /// <summary>
    /// Общее количество опубликованных сообщений пользователем.
    /// </summary>
    public int TotalMessagesPosted { get; set; } = 0;

    /// <summary>
    /// Общее количество лайков, которые пользователь поставил на сообщения других.
    /// </summary>
    public int TotalMessagesLiked { get; set; } = 0;

    /// <summary>
    /// Количество лайков на текущем сообщении.
    /// </summary>
    public int LikesOnThisMessage { get; set; } = 0;
}