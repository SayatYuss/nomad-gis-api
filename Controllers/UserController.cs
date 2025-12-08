using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Users; // Подключаем DTO
using nomad_gis_V2.Exceptions; // Подключаем кастомные исключения

namespace nomad_gis_V2.Controllers
{
    /// <summary>
    /// API контроллер для управления пользователями (только для администраторов).
    /// Предоставляет endpoints для получения информации о пользователях, изменения ролей и удаления.
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    [Authorize(Roles = "Admin")] // <-- Весь контроллер только для Админов
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр класса UsersController.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения</param>
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить список всех пользователей.
        /// </summary>
        /// <returns>Список пользователей с базовой информацией</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponse // Маппим в DTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Получить детальную информацию о пользователе.
        /// Включает историю открытых точек и достигнутых ачивок.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Детальная информация о пользователе с прогрессом</returns>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserDetails(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.MapProgress) // Включаем прогресс по точкам
                    .ThenInclude(mp => mp.MapPoint) // И саму информацию о точке
                .Include(u => u.UserAchievements) // Включаем прогресс по ачивкам
                    .ThenInclude(ua => ua.Achievement) // И саму информацию о ачивке
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            // Маппим вручную в наш новый DTO
            var userDetail = new UserDetailResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                AvatarUrl = user.AvatarUrl,
                Level = user.Level,
                Experience = user.Experience,

                UnlockedPoints = user.MapProgress.Select(mp => new UserPointProgressDto
                {
                    MapPointId = mp.MapPointId,
                    MapPointName = mp.MapPoint.Name,
                    UnlockedAt = mp.UnlockedAt
                }).OrderByDescending(p => p.UnlockedAt).ToList(),

                Achievements = user.UserAchievements.Select(ua => new UserAchProgressDto
                {
                    AchievementId = ua.AchievementId,
                    AchievementTitle = ua.Achievement.Title,
                    IsCompleted = ua.IsCompleted,
                    CompletedAt = ua.CompletedAt
                }).OrderByDescending(a => a.CompletedAt).ToList()
            };

            return Ok(userDetail);
        }

        /// <summary>
        /// Изменить роль пользователя.
        /// Позволяет назначать пользователю роль Admin или User.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="request">Новая роль пользователя</param>
        /// <returns>Сообщение об успешном обновлении</returns>
        [HttpPut("{id}/role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            if (request.Role != "Admin" && request.Role != "User")
            {
                throw new ValidationException("Role must be 'Admin' or 'User'");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            user.Role = request.Role;
            user.UpdatedAt = DateTime.UtcNow; // (Если ты не настроил авто-обновление)
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {user.Username} role updated to {user.Role}" });
        }

        /// <summary>
        /// Удалить пользователя и все связанные данные.
        /// Удаляет сообщения, прогресс, ачивки и другие данные пользователя.
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Сообщение об успешном удалении</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            // Благодаря DeleteBehavior.Cascade в твоем ApplicationDbContext,
            // EF Core автоматически удалит связанные Messages, RefreshTokens и т.д.

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User {user.Username} and all related data deleted" });
        }
    }
}