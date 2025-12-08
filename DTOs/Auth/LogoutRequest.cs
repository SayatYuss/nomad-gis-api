namespace nomad_gis_V2.DTOs.Auth;

/// <summary>
/// DTO запроса на выход из аккаунта (инвалидация refresh токена).
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// ID пользователя, выходящего из системы.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Refresh токен для инвалидации.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// ID устройства, с которого выполняется выход.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
}