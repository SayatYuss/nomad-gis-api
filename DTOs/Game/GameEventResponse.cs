using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Messages;

namespace nomad_gis_V2.DTOs.Game;

/// <summary>
/// DTO ответа на событие в игре (проверка местоположения, разблокировка точек).
/// Содержит информацию об изменениях в профиле, уровне, ачивках и новых сообщениях.
/// </summary>
public class GameEventResponse
{
    /// <summary>
    /// Флаг успешности операции (точка разблокирована или нет).
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Сообщение о результате проверки местоположения.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Количество опыта, полученного при выполнении события.
    /// </summary>
    public int ExperienceGained { get; set; } = 0;

    /// <summary>
    /// Флаг, указывающий на повышение уровня пользователя.
    /// </summary>
    public bool LeveledUp { get; set; } = false;

    /// <summary>
    /// Обновленные данные пользователя после события.
    /// </summary>
    public UserDto? UserData { get; set; }

    /// <summary>
    /// Список ачивок, разблокированных при этом событии.
    /// </summary>
    public List<AchievementResponse> UnlockedAchievements { get; set; } = new();

    /// <summary>
    /// ID разблокированной точки карты.
    /// </summary>
    public Guid? UnlockedPointId { get; set; }

    /// <summary>
    /// Автоматически созданное сообщение при разблокировке точки.
    /// </summary>
    public MessageResponse? CreatedMessage { get; set; }

    /// <summary>
    /// Статус лайка для созданного сообщения.
    /// </summary>
    public bool? IsLiked { get; set; }
}
