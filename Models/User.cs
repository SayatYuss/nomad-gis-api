using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель пользователя приложения.
/// Содержит учетные данные, профиль и связанные данные (прогресс, ачивки, сообщения).
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Имя пользователя (логин).
    /// </summary>
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Хэш пароля пользователя.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Широта последнего известного местоположения пользователя.
    /// </summary>
    [Range(-90.0, 90.0)]
    public double? Latitude { get; set; }

    /// <summary>
    /// Долгота последнего известного местоположения пользователя.
    /// </summary>
    [Range(-180.0, 180.0)]
    public double? Longitude { get; set; }

    /// <summary>
    /// Общее количество опыта пользователя.
    /// </summary>
    public int Experience { get; set; } = 0;

    /// <summary>
    /// Текущий уровень пользователя.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Роль пользователя (User или Admin).
    /// </summary>
    [Required, MaxLength(50)]
    public string Role { get; set; } = "User"; // По умолчанию "User"

    /// <summary>
    /// URL аватара пользователя.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Дата и время создания учетной записи.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата и время последнего обновления профиля.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Флаг активности учетной записи.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Прогресс пользователя по открытию точек на карте.
    /// </summary>
    public virtual ICollection<UserMapProgress> MapProgress { get; set; } = new List<UserMapProgress>();

    /// <summary>
    /// Ачивки, полученные пользователем.
    /// </summary>
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    /// <summary>
    /// Сообщения, опубликованные пользователем.
    /// </summary>
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    /// <summary>
    /// Refresh токены для аутентификации пользователя.
    /// </summary>
    public List<RefreshToken> RefreshTokens { get; set; } = new(5);
}
