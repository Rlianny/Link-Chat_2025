namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;
    // manages sending and receiving files, acknowledgements and sending retries
    public interface IFileTransferService
    {
        public Task SendFileStart(FileStart fileStart);
        public Task SendFileChunk(FileChunk chunk);
        public Task SendFile(string receiverUserName, string filePath);
        public File GetFileById(string messageId);
        public event Action<File> FileFrameReceived;
    }
}