namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // manages raw sockets, P/Invoke, and sending and receiving frames
    public interface INetworkService
    {
        public void SendMessage(User user, Message message);
        public event Action<byte[]>? HeartbeatFrameReceived;
        public event Action<byte[]>? ChatAckFrameReceived;
        public event Action<byte[]>? FileAckFrameReceived;
        public event Action<byte[]>? TextMessageFrameReceived;
        public event Action<byte[]>? FileStartFrameReceived;
        public event Action<byte[]>? FileChunkFrameReceived;
        public event Action<byte[]>? MessageReactionFrameReceived;
        public event Action<byte[]>? UserStatusFrameReceived;
    }
}