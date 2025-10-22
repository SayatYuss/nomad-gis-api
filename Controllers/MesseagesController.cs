using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Messages;
using nomad_gis_V2.Interfaces;
using System.Security.Claims;

namespace nomad_gis_V2.Controllers
{
    [ApiController]
    [Route("api/v1/messages")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // POST api/messages/add
        [HttpPost("add")]
        public async Task<IActionResult> AddMessage([FromBody] MessageRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _messageService.AddMessageAsync(userId, request);
            return Ok(result);
        }

        // GET api/messages/list?pointId=...
        [HttpGet("list")]
        [AllowAnonymous] // можно сделать публичным, если хочешь
        public async Task<IActionResult> GetMessages([FromQuery] Guid pointId)
        {
            var result = await _messageService.GetMessagesByPointAsync(pointId);
            return Ok(result);
        }
    }
}
