using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Messages;
using nomad_gis_V2.Exceptions;
using nomad_gis_V2.Interfaces;
using System.Security.Claims;

namespace nomad_gis_V2.Controllers
{
    /// <summary>
    /// API контроллер для управления сообщениями и комментариями на точках.
    /// Предоставляет endpoints для получения, создания, удаления сообщений и проставления лайков.
    /// </summary>
    [ApiController]
    [Route("api/v1/messages")]
    [Authorize]
    public class MesseagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        /// <summary>
        /// Инициализирует новый экземпляр класса MesseagesController.
        /// </summary>
        /// <param name="messageService">Сервис управления сообщениями</param>
        public MesseagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        /// <summary>
        /// Получить все сообщения на конкретной точке карты.
        /// Возвращает все комментарии пользователей для выбранной точки.
        /// </summary>
        /// <param name="pointId">ID точки на карте</param>
        /// <returns>Список сообщений с информацией об авторе, содержании и лайках</returns>
        [HttpGet("point/{pointId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMessagesByPointId(Guid pointId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var messages = await _messageService.GetMessagesByPointIdAsync(pointId, userId);
            return Ok(messages);
        }

        /// <summary>
        /// Создать новое сообщение на точке карты.
        /// Добавляет комментарий от текущего пользователя на выбранную точку.
        /// </summary>
        /// <param name="dto">Содержание сообщения и ID точки</param>
        /// <returns>Созданное сообщение с информацией об авторе и времени создания</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateMessage([FromBody] MessageRequest dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _messageService.CreateMessageAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// Удалить сообщение (только автор может удалить свое сообщение).
        /// </summary>
        /// <param name="id">ID сообщения для удаления</param>
        /// <returns>Сообщение об успешном удалении</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _messageService.DeleteMessageAsync(id, userId);

            if (!success)
                throw new NotFoundException("Message not found or you don't have permission to delete it.");

            return NoContent();
        }

        /// <summary>
        /// Удалить сообщение от имени администратора (без проверки авторства).
        /// Позволяет администраторам удалять любые сообщения.
        /// </summary>
        /// <param name="id">ID сообщения для удаления</param>
        /// <returns>Сообщение об успешном удалении</returns>
        // --- НОВЫЙ МЕТОД ДЛЯ АДМИНА ---
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminDeleteMessage(Guid id)
        {
            // Этот метод удаляет сообщение, не проверяя, кто автор
            var success = await _messageService.AdminDeleteMessageAsync(id);

            if (!success)
                throw new NotFoundException("Message not found.");

            return Ok(new { message = "Message deleted by admin" });
        }

        /// <summary>
        /// Поставить или убрать лайк на сообщение.
        /// Переключает состояние лайка (ставит лайк, если его нет, или удаляет, если уже стоит).
        /// </summary>
        /// <param name="id">ID сообщения для лайка</param>
        /// <returns>Информация о количестве лайков после операции</returns>
        [HttpPost("{id}/like")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LikeMessage(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _messageService.ToggleLikeAsync(id, userId);

            return Ok(result);
        }
    }
}