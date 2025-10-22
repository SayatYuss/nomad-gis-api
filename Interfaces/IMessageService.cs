using nomad_gis_V2.DTOs.Messages;

namespace nomad_gis_V2.Interfaces;

public interface IMessageService
{
    Task<MessageResponse> CreateMessageAsync(Guid userId, MessageRequest request);
    Task<IEnumerable<MessageResponse>> GetMessagesByPointIdAsync(Guid mapPointId);
    Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
    Task<bool> AdminDeleteMessageAsync(Guid messageId);
}

