namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;
    // manages sending and receiving files, acknowledgements and sending retries
    public interface IFileTransferService
    {
        public void SendFile(string receiverUserName, string filePath);
        public File GetFileById(string messageId);
        public event Action<File> FileFrameReceived;
        public event Action<File, string> FileSended;
    }
}