using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace nomad_gis_V2.Models;

/// <summary>
/// Модель лайка на сообщение.
/// Связывает пользователя с сообщением, которое он лайкнул.
/// </summary>
[PrimaryKey(nameof(UserId), nameof(MessageId))]
public class MessageLike()
{
    /// <summary>
    /// ID пользователя, поставившего лайк.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь, поставивший лайк (навигационное свойство).
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// ID сообщения, на которое поставлен лайк.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Сообщение, на которое поставлен лайк (навигационное свойство).
    /// </summary>
    public virtual Message Message { get; set; } = null!;

    /// <summary>
    /// Дата и время добавления лайка.
    /// </summary>
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}