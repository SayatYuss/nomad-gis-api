using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Points;

/// <summary>
/// DTO запроса на обновление информации о точке на карте (только для администраторов).
/// </summary>
public class MapPointUpdateRequest
{
    /// <summary>
    /// Новое название точки интереса.
    /// </summary>
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Новая широта географического расположения точки (от -90 до 90).
    /// </summary>
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    /// <summary>
    /// Новая долгота географического расположения точки (от -180 до 180).
    /// </summary>
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    /// <summary>
    /// Новый радиус разблокировки точки в метрах.
    /// </summary>
    [Required]
    [Range(1, double.MaxValue)]
    public double UnlockRadiusMeters { get; set; }

    /// <summary>
    /// Новое описание точки интереса.
    /// </summary>
    public string? Description { get; set; }
}
