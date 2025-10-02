namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;
    // manages sending and receiving files, acknowledgements and sending retries
    public interface IFileTransferService
    {
        public void SendFile(string userName, string filePath);
        public File GetFileById(int message);
        public event Action<File> FileFrameReceived;
    }
}