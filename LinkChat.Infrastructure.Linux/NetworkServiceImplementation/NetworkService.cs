using LinkChat.Core.Models;
using LinkChat.Core.Services;

namespace LinkChat.Infrastructure
{
    public sealed class LinuxNetworkService : INetworkService, IDisposable
    {
        public event Action<byte[]>? HeartbeatFrameReceived;
        public event Action<byte[]>? ChatAckFrameReceived;
        public event Action<byte[]>? FileAckFrameReceived;
        public event Action<byte[]>? TextMessageFrameReceived;
        public event Action<byte[]>? FileStartFrameReceived;
        public event Action<byte[]>? FileChunkFrameReceived;
        public event Action<byte[]>? MessageReactionFrameReceived;
        public event Action<byte[]>? UserStatusFrameReceived;
        public void SendMessage(User user, Message message)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}