namespace nomad_gis_V2.DTOs.Messages;

/// <summary>
/// DTO ответа с информацией о сообщении на точке карты.
/// Содержит информацию о сообщении, авторе и статусе лайков.
/// </summary>
public class MessageResponse
{
    /// <summary>
    /// Уникальный ID сообщения.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Текстовое содержание сообщения.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время публикации сообщения.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// ID автора сообщения.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Имя пользователя (автора сообщения).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// URL аватара автора сообщения.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// ID точки, на которой размещено сообщение.
    /// </summary>
    public Guid MapPointId { get; set; }

    /// <summary>
    /// Количество лайков, полученных сообщением.
    /// </summary>
    public int LikesCount { get; set; }

    /// <summary>
    /// Флаг, указывающий, лайкнул ли текущий пользователь это сообщение.
    /// </summary>
    public bool IsLikedByCurrentUser { get; set; }
}
