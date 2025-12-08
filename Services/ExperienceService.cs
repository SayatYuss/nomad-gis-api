using nomad_gis_V2.Helpers;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services;

/// <summary>
/// Сервис для управления опытом и уровнем пользователя.
/// Добавляет опыт и отслеживает повышение уровня.
/// </summary>
public class ExperienceService : IExperienceService
{
    /// <summary>
    /// Асинхронно добавляет опыт пользователю и проверяет повышение уровня.
    /// </summary>
    /// <param name="user">Пользователь, которому добавляется опыт</param>
    /// <param name="amount">Количество опыта для добавления</param>
    /// <returns>True если операция успешна, false если пользователь null или amount &lt;= 0</returns>
    public Task<bool> AddExperienceAsync(User user, int amount)
    {
        if (user == null || amount <= 0)
        {
            return Task.FromResult(false);
        }

        user.Experience += amount;

        bool leveledUp = false;

        int requiredXp = LevelCalculator.GetRequiredExperience(user.Level);

        while (user.Experience >= requiredXp)
        {
            user.Level += 1;
            user.Experience -= requiredXp;
            leveledUp = true;

            requiredXp = LevelCalculator.GetRequiredExperience(user.Level);
        }

        return Task.FromResult(leveledUp);
    }
}