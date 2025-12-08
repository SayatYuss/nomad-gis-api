using Microsoft.EntityFrameworkCore;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель, отслеживающая прогресс пользователя по открытию точек на карте.
/// Фиксирует момент времени, когда пользователь разблокировал определенную точку.
/// </summary>
[PrimaryKey(nameof(UserId), nameof(MapPointId))]
public class UserMapProgress
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
    /// ID точки на карте.
    /// </summary>
    public Guid MapPointId { get; set; }

    /// <summary>
    /// Точка на карте (навигационное свойство).
    /// </summary>
    public virtual MapPoint MapPoint { get; set; } = null!;

    /// <summary>
    /// Дата и время разблокировки точки пользователем.
    /// </summary>
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}
