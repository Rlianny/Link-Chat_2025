namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // parses the received frames and constructs the frames to be sent
    public interface IProtocolService
    {
        public Message? ParseFrame(byte[] frame);

        public byte[] CreateFrame(Message message);
    }
}