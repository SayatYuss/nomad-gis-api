namespace nomad_gis_V2.DTOs.Messages;

/// <summary>
/// DTO запроса на создание нового сообщения на точке карты.
/// </summary>
public class MessageRequest
{
    /// <summary>
    /// ID точки, на которой публикуется сообщение.
    /// </summary>
    public Guid MapPointId { get; set; }

    /// <summary>
    /// Текстовое содержание сообщения.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}