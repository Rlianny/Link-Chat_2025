using System.Text;
using System.Text.Json;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class ProtocolService : IProtocolService
{
    public event Action<HeartbeatMessage>? HeartbeatFrameReceived;
    public event Action<ChatAck>? ChatAckFrameReceived;
    public event Action<FileAck>? FileAckFrameReceived;
    public event Action<TextMessage>? TextMessageFrameReceived;
    public event Action<FileStart>? FileStartFrameReceived;
    public event Action<FileChunk>? FileChunkFrameReceived;
    public event Action<MessageReaction>? MessageReactionFrameReceived;
    public event Action<UserStatus>? UserStatusFrameReceived;
    public event Action<byte[]>? FrameReadyToSend;

    public ProtocolService(INetworkService networkService)
    {
        networkService.FrameReceived += OnFrameReceived;
    }
    public byte[] CreateFrame(User receiver, Message message)
    {
        byte[] destMacAddress = receiver.MacAddress;
        byte[] localMacAddress;

        return destMacAddress;
    }
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

    private void OnFrameReceived(byte[] frameData)
    {
        Message? message = ParseFrame(frameData);

        switch (message)
        {
            case ChatAck chatAck:
                ChatAckFrameReceived?.Invoke(chatAck);
                break;

            case FileAck fileAck:
                FileAckFrameReceived?.Invoke(fileAck);
                break;

            case FileChunk fileChunk:
                FileChunkFrameReceived?.Invoke(fileChunk);
                break;

            case FileStart fileStart:
                FileStartFrameReceived?.Invoke(fileStart);
                break;

            case HeartbeatMessage heartbeatMessage:
                HeartbeatFrameReceived?.Invoke(heartbeatMessage);
                break;

            case MessageReaction messageReaction:
                MessageReactionFrameReceived?.Invoke(messageReaction);
                break;

            case TextMessage textMessage:
                TextMessageFrameReceived?.Invoke(textMessage);
                break;

            case UserStatus userStatus:
                UserStatusFrameReceived?.Invoke(userStatus);
                break;

            default:
                throw new Exception("The type of parsed message cannot be identified.");
        }
    }
}