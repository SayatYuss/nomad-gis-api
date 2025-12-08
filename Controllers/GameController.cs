using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.Interfaces;
using System.Security.Claims;

namespace nomad_gis_V2.Controllers
{
    /// <summary>
    /// API контроллер для геймплея.
    /// Обрабатывает проверку местоположения пользователя и разблокировку точек на карте.
    /// </summary>
    [ApiController]
    [Route("api/v1/game")]
    [Authorize] // Доступно только авторизованным
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        /// <summary>
        /// Инициализирует новый экземпляр класса GameController.
        /// </summary>
        /// <param name="gameService">Сервис игровой логики</param>
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Проверить местоположение пользователя и разблокировать близлежащие точки.
        /// Проверяет, находится ли пользователь в пределах допустимого расстояния от точек на карте
        /// и разблокирует их, если условия выполнены. Также может активировать ачивки.
        /// </summary>
        /// <param name="request">Текущие координаты пользователя (широта и долгота)</param>
        /// <returns>Информация о разблокированных точках и активированных ачивках</returns>
        [HttpPost("check-location")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckLocation([FromBody] CheckLocationRequest request)
        {
            // Берем ID пользователя из его JWT-токена
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _gameService.CheckAndUnlockPointsAsync(userId, request);

            if (!result.Success)
            {
                return Ok(result); // Все равно Ok, просто с сообщением "ничего не найдено"
            }

            return Ok(result); // Возвращаем DTO с успехом
        }
    }
}