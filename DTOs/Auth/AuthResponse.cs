namespace nomad_gis_V2.DTOs.Auth;

/// <summary>
/// DTO ответа при успешной аутентификации пользователя.
/// Содержит access и refresh токены, а также информацию о пользователе.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Access токен для доступа к защищенным ресурсам API.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh токен для получения нового access токена после истечения.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Информация об авторизованном пользователе.
    /// </summary>
    public UserDto User { get; set; } = new UserDto();
}

/// <summary>
/// DTO информации о пользователе.
/// Используется в ответах аутентификации и профиля.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Уникальный ID пользователя.
    /// </summary>
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (логин).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Количество опыта пользователя.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Текущий уровень пользователя.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// URL аватара пользователя.
    /// </summary>
    public string? AvatarUrl { get; set; }
}