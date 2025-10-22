using System.ComponentModel.DataAnnotations;

namespace nomad_gis_V2.Models;

public class MapPoint
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required]
    public double UnlockRadiusMeters { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Добавь навигацию для сообщений (одна точка -> много сообщений)
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    // Навигация на прогресс (одна точка -> много открытий пользователями)
    public virtual ICollection<UserMapProgress> UserProgress { get; set; } = new List<UserMapProgress>();
}
