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

        public async Task<MessageResponse> CreateMessageAsync(Guid userId, MessageRequest request)
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

        public async Task<IEnumerable<MessageResponse>> GetMessagesByPointIdAsync(Guid mapPointId, Guid currentUserId)
        {
            var messages = await _context.Messages
                .Where(m => m.MapPointId == mapPointId)
                .Include(m => m.User)
                .Include(m => m.Likes)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                UserId = m.UserId,
                Username = m.User.Username,
                MapPointId = m.MapPointId,
                LikesCount = m.Likes.Count,
                IsLikedByCurrentUser = m.Likes.Any(l => l.UserId == currentUserId)
            });
        }

        public async Task<bool> AdminDeleteMessageAsync(Guid messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null || message.UserId != userId)
            {
                return false;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleLikeAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null) throw new Exception("Message not found!");

            var existingLike = await _context.MessageLikes
                .FindAsync(userId, messageId);

            if (existingLike != null)
            {
                _context.MessageLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false;
            }

            var newLike = new MessageLike
            {
                UserId = userId,
                MessageId = messageId
            };

            _context.MessageLikes.Add(newLike);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
