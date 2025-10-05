namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // parses the received frames and constructs the frames to be sent
    public interface IProtocolService
    {
        public Message? ParseFrame(byte[] frame);
        public byte[] CreateFrameToSend(User receiver, Message message, bool broadcast);
        public event Action<HeartbeatMessage>? HeartbeatFrameReceived;
        public event Action<ChatAck>? ChatAckFrameReceived;
        public event Action<FileChunkAck>? FileChunkAckFrameReceived;
        public event Action<FileStartAck>? FileStartAckFrameReceived;
        public event Action<TextMessage>? TextMessageFrameReceived;
        public event Action<FileStart>? FileStartFrameReceived;
        public event Action<FileChunk>? FileChunkFrameReceived;
        public event Action<MessageReaction>? MessageReactionFrameReceived;
        public event Action<UserStatus>? UserStatusFrameReceived;
    }
}