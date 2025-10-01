namespace LinkChat.Core
{
    // parses the received frames and constructs the frames to be sent
    public interface IProtocolService
    {
        public Message? ParseFrame(byte[] frame);

        public byte[] CreateHeartbeatFrame(HeartbeatMessage heartbeat);
        public byte[] CreateUserStatusFrame(UserStatus userStatus);
        public byte[] CreateChatMessageFrame(ChatMessage chatMessage);
        public byte[] CreateChatAckFrame(ChatAck chatAck);
        public byte[] CreateMessageReactionFrame(MessageReaction messageReaction);
        public byte[] CreateFileStartFrame(FileStart fileStart);
        public byte[] CreateFileChunkFrame(FileChunk fileChunk);
        public byte[] CreateFileAckFrame(FileAck fileAck);
    }
}