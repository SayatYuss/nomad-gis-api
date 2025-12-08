using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель refresh токена для аутентификации пользователя.
/// Используется для получения новых access токенов без повторной аутентификации.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Уникальный идентификатор токена.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Значение refresh токена.
    /// </summary>
    [Required, MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время истечения токена.
    /// </summary>
    [Required]
    public DateTime Expires { get; set; }

    /// <summary>
    /// Дата и время создания токена.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата и время отзыва токена (если он был аннулирован).
    /// </summary>
    public DateTime? RevorkedAt { get; set; }

    /// <summary>
    /// ID устройства, для которого выдан токен.
    /// </summary>
    [Required, MaxLength(200)]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// ID пользователя, которому принадлежит токен.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь, которому принадлежит токен (навигационное свойство).
    /// </summary>
    public User User { get; set; } = null!;
}
