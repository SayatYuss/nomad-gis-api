using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Data;
using nomad_gis_V2.DTOs.Messages;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MessageResponse> AddMessageAsync(Guid userId, MessageRequest request)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new Exception("Пользователь не найден.");

            var point = await _context.MapPoints.FindAsync(request.MapPointId)
                ?? throw new Exception("Точка не найдена.");

            var message = new Message
            {
                Content = request.Content,
                UserId = userId,
                MapPointId = request.MapPointId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return new MessageResponse
            {
                Id = message.Id,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                UserId = user.Id,
                Username = user.Username,
                MapPointId = message.MapPointId
            };
        }

        public async Task<IEnumerable<MessageResponse>> GetMessagesByPointAsync(Guid mapPointId)
        {
            var messages = await _context.Messages
                .Where(m => m.MapPointId == mapPointId)
                .Include(m => m.User)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                UserId = m.UserId,
                Username = m.User.Username,
                MapPointId = m.MapPointId
            });
        }
    }
}
