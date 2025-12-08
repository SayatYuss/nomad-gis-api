using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.DTOs.Auth;

/// <summary>
/// DTO запроса на регистрацию нового пользователя.
/// Содержит данные профиля и информацию об устройстве.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Электронная почта нового пользователя.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя (минимум 8 символов).
    /// </summary>
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (логин).
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный ID устройства для создания refresh токена.
    /// </summary>
    [Required]
    public string DeviceId { get; set; } = string.Empty;
}