namespace nomad_gis_V2.DTOs.Points;

/// <summary>
/// DTO информации о точке на карте для передачи клиенту.
/// Используется для отображения точек и их деталей.
/// </summary>
public class MapPointRequest
{
    /// <summary>
    /// Уникальный ID точки.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название точки интереса.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Широта географического расположения точки.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Долгота географического расположения точки.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Радиус разблокировки точки в метрах.
    /// </summary>
    public double UnlockRadiusMeters { get; set; }

    /// <summary>
    /// Описание точки интереса.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время создания точки.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
