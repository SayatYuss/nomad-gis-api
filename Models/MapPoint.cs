using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель точки интереса на карте.
/// Содержит географические координаты, радиус разблокировки и связанные сообщения.
/// </summary>
public class MapPoint
{
    /// <summary>
    /// Уникальный идентификатор точки.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Название точки интереса.
    /// </summary>
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Географическое местоположение точки (широта и долгота).
    /// Хранится в формате Geography (координаты WGS84).
    /// </summary>
    [Required]
    [Column(TypeName = "geography(Point, 4326)")]
    public Point Location { get; set; } = null!;

    /// <summary>
    /// Радиус разблокировки в метрах.
    /// Пользователь может открыть точку, если находится на расстоянии не более этого значения.
    /// </summary>
    [Required]
    public double UnlockRadiusMeters { get; set; }

    /// <summary>
    /// Описание точки интереса.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время создания точки.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Сообщения и комментарии на этой точке.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    /// <summary>
    /// Прогресс пользователей по открытию этой точки.
    /// </summary>
    public virtual ICollection<UserMapProgress> UserProgress { get; set; } = new List<UserMapProgress>();
}
