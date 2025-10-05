using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
using LinkChat.Core.Tools;
namespace LinkChat.Core.Implementations;

public class ProtocolService : IProtocolService
{
    private INetworkService networkService;
    public event Action<HeartbeatMessage>? HeartbeatFrameReceived;
    public event Action<ChatAck>? ChatAckFrameReceived;
    public event Action<FileChunkAck>? FileChunkAckFrameReceived;
    public event Action<TextMessage>? TextMessageFrameReceived;
    public event Action<FileStart>? FileStartFrameReceived;
    public event Action<FileChunk>? FileChunkFrameReceived;
    public event Action<MessageReaction>? MessageReactionFrameReceived;
    public event Action<UserStatus>? UserStatusFrameReceived;
    public event Action<byte[]>? FrameReadyToSend;
    public event Action<FileStartAck>? FileStartAckFrameReceived;

    private readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProtocolService(INetworkService networkService)
    {
        this.networkService = networkService;
        this.networkService.FrameReceived += OnFrameReceived;
    }
    public byte[] CreateFrameToSend(User? receiver, Message message, bool broadcast = false)
    {
        byte[] destMacAddress = new byte[6];
        if (broadcast)
            destMacAddress = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];
        else if (receiver is null)
            throw new Exception("The message receiver cannot be null, but broadcast");
        else
            destMacAddress = receiver.MacAddress;


        byte[] header = new byte[14];
        byte[] localMacAddress = Tools.Tools.GetLocalMacAddress();

        Buffer.BlockCopy(destMacAddress, 0, header, 0, 6); // Copy the destination MAC (first 6 bytes).
        Buffer.BlockCopy(localMacAddress, 0, header, 6, 6); // Copy our source MAC (next 6 bytes).

        byte[] etherTypeBytes = BitConverter.GetBytes(Tools.Tools.htons(Tools.Tools.ETHER_TYPE));
        Buffer.BlockCopy(etherTypeBytes, 0, header, 12, 2); // Copy our app ether type.

        byte[] payload = GetPayload(message);

        byte[] frame = new byte[header.Length + payload.Length];
        Buffer.BlockCopy(header, 0, frame, 0, header.Length);
        Buffer.BlockCopy(payload, 0, frame, header.Length, payload.Length);

        return frame;
    }

    private byte[] GetPayload(Message message)
    {
        string json = JsonSerializer.Serialize(message, options);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        byte[] encryptedBytes = AesEncryptor.Encrypt(bytes);
        return encryptedBytes;
    }

    public Message? ParseFrame(byte[] frame)
    {
        byte[] encryptedPayload = new byte[frame.Length - 14];
        Buffer.BlockCopy(frame, 14, encryptedPayload, 0, encryptedPayload.Length);

        try
        {
            byte[] decryptedBytes = AesEncryptor.Decrypt(encryptedPayload);
            string recoveryMessage = Encoding.UTF8.GetString(decryptedBytes);
            Message? message = JsonSerializer.Deserialize<Message>(recoveryMessage, options);
            return message;
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("WARNING: Frame received with corrupted data or wrong key (decrypting failure).");
            Console.WriteLine(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing the decrypted message: {ex.Message}");
            return null;
        }

    }

    private void OnFrameReceived(byte[] frameData)
    {
        Message? message = ParseFrame(frameData);

        switch (message)
        {
            case ChatAck chatAck:
                ChatAckFrameReceived?.Invoke(chatAck);
                break;

            case FileChunkAck fileChunkAck:
                FileChunkAckFrameReceived?.Invoke(fileChunkAck);
                break;

            case FileStartAck fileStartAck:
                FileStartAckFrameReceived?.Invoke(fileStartAck);
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