namespace nomad_gis_V2.DTOs.Leaderboard;

/// <summary>
/// DTO записи в лидерборде (рейтинге).
/// Содержит информацию о позиции игрока в рейтинге.
/// </summary>
public class LeaderboardEntryDto
{
    /// <summary>
    /// Позиция игрока в рейтинге (1-10).
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Уникальный ID пользователя.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// URL аватара пользователя.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Уровень пользователя.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Показатель для рейтинга (опыт, количество открытых точек или ачивок в зависимости от типа лидербоарда).
    /// </summary>
    public int Score { get; set; }
}