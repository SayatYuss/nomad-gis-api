namespace nomad_gis_V2.DTOs.Auth;

/// <summary>
/// DTO запроса на обновление access токена используя refresh токен.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// ID пользователя, для которого нужно обновить токен.
    /// </summary>
    public Guid UserId { get; set; } = Guid.Empty;

    /// <summary>
    /// Refresh токен для получения нового access токена.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// ID устройства, для которого обновляется токен.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;
}