namespace nomad_gis_V2.Profile;

/// <summary>
/// DTO запроса на обновление профиля пользователя.
/// Позволяет изменять имя пользователя, пароль и аватар.
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// Новое имя пользователя (логин).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Текущий пароль для подтверждения при изменении пароля.
    /// </summary>
    public string? CurrentPassword { get; set; }

    /// <summary>
    /// Новый пароль (если требуется изменение).
    /// </summary>
    public string? NewPassword { get; set; }

    /// <summary>
    /// Файл нового аватара для загрузки.
    /// </summary>
    public IFormFile? AvatarFile { get; set; }
}