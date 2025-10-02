namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // manages raw sockets, P/Invoke, and sending and receiving frames
    public interface INetworkService
    {
        public Task SendFrameAsync(byte[] frame);
        public void StartListening();
        public event Action<byte[]>? FrameReceived;
    }
}