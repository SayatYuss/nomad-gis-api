using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Points;

/// <summary>
/// DTO запроса на создание новой точки на карте (только для администраторов).
/// Содержит все необходимые данные для создания точки интереса.
/// </summary>
public class MapPointCreateRequest
{
    /// <summary>
    /// Название новой точки интереса.
    /// </summary>
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Широта географического расположения точки (от -90 до 90).
    /// </summary>
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    /// <summary>
    /// Долгота географического расположения точки (от -180 до 180).
    /// </summary>
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    /// <summary>
    /// Радиус разблокировки точки в метрах (должен быть больше 1).
    /// </summary>
    [Required]
    [Range(1, double.MaxValue)]
    public double UnlockRadiusMeters { get; set; }

    /// <summary>
    /// Описание точки интереса.
    /// </summary>
    public string? Description { get; set; }
}
