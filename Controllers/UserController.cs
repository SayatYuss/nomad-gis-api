using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Users; // Подключаем DTO
using nomad_gis_V2.Exceptions; // Подключаем кастомные исключения

namespace nomad_gis_V2.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize(Roles = "Admin")] // <-- Весь контроллер только для Админов
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        [HttpGet]
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
        /// Изменить роль пользователя
        /// </summary>
        [HttpPut("{id}/role")]
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
        /// Удалить пользователя
        /// </summary>
        [HttpDelete("{id}")]
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