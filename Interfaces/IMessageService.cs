using nomad_gis_V2.DTOs.Messages;

namespace nomad_gis_V2.Interfaces;

public interface IMessageService
{
    Task<MessageResponse> AddMessageAsync(Guid userId, MessageRequest request);
    Task<IEnumerable<MessageResponse>> GetMessagesByPointAsync(Guid mapPointId);
}

