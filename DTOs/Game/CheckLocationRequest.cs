using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Game;

/// <summary>
/// DTO запроса для проверки местоположения пользователя.
/// Содержит GPS координаты для проверки близости к точкам на карте.
/// </summary>
public class CheckLocationRequest
{
    /// <summary>
    /// Широта текущего местоположения пользователя (от -90 до 90).
    /// </summary>
    [Required, Range(-90, 90)]
    public double Latitude { get; set; }

    /// <summary>
    /// Долгота текущего местоположения пользователя (от -180 до 180).
    /// </summary>
    [Required, Range(-180, 180)]
    public double Longitude { get; set; }
}