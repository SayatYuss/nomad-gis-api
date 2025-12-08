using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель сообщения (комментария) на точке карты.
/// Содержит текст, информацию об авторе и связанные лайки.
/// </summary>
public class Message
{
    /// <summary>
    /// Уникальный идентификатор сообщения.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID автора сообщения.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Автор сообщения (навигационное свойство).
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// ID точки карты, на которой размещено сообщение.
    /// </summary>
    [Required]
    public Guid MapPointId { get; set; }

    /// <summary>
    /// Точка карты, на которой размещено сообщение (навигационное свойство).
    /// </summary>
    [ForeignKey(nameof(MapPointId))]
    public virtual MapPoint Point { get; set; } = null!;

    /// <summary>
    /// Текстовое содержание сообщения.
    /// </summary>
    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время публикации сообщения.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Лайки, полученные этим сообщением.
    /// </summary>
    public ICollection<MessageLike> Likes { get; set; } = new List<MessageLike>();
}
