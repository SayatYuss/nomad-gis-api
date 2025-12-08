namespace nomad_gis_V2.DTOs.Auth;

/// <summary>
/// DTO запроса на вход в аккаунт.
/// Содержит учетные данные пользователя и ID устройства.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Электронная почта или имя пользователя для входа.
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Уникальный ID устройства для создания refresh токена.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
