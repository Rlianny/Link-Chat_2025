using System.Text;
using System.Text.Json;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class ProtocolService : IProtocolService
{
    public byte[] CreateFrame(Message message)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        string json = JsonSerializer.Serialize(message, options);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return bytes;
    }

    public Message? ParseFrame(byte[] frame)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        string RecoveryMessage = Encoding.UTF8.GetString(frame);
        Message Message = JsonSerializer.Deserialize<Message>(RecoveryMessage, options);
        return Message;
    }
}