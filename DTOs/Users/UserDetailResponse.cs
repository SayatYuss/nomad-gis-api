using System;
using System.Collections.Generic;

namespace nomad_gis_V2.DTOs.Users;

/// <summary>
/// DTO ответа с полной информацией о пользователе для администратора.
/// Включает статистику, открытые точки и достижения.
/// </summary>
public class UserDetailResponse
{
    /// <summary>
    /// Уникальный ID пользователя.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя пользователя (логин).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя (User или Admin).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время создания учетной записи.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Флаг активности учетной записи.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// URL аватара пользователя.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Текущий уровень пользователя.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Количество опыта пользователя.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Список открытых пользователем точек на карте.
    /// </summary>
    public List<UserPointProgressDto> UnlockedPoints { get; set; } = new();

    /// <summary>
    /// Список достижений пользователя.
    /// </summary>
    public List<UserAchProgressDto> Achievements { get; set; } = new();
}

/// <summary>
/// DTO краткой информации об открытой пользователем точке на карте.
/// </summary>
public class UserPointProgressDto
{
    /// <summary>
    /// ID открытой точки.
    /// </summary>
    public Guid MapPointId { get; set; }

    /// <summary>
    /// Название открытой точки.
    /// </summary>
    public string MapPointName { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время открытия точки.
    /// </summary>
    public DateTime UnlockedAt { get; set; }
}

/// <summary>
/// DTO информации о достижении пользователя.
/// </summary>
public class UserAchProgressDto
{
    /// <summary>
    /// ID достижения.
    /// </summary>
    public Guid AchievementId { get; set; }

    /// <summary>
    /// Название достижения.
    /// </summary>
    public string AchievementTitle { get; set; } = string.Empty;

    /// <summary>
    /// Флаг, указывающий, завершено ли достижение.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Дата и время завершения достижения.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
