namespace LinkChat.Core.Services
{
    // manages sending and receiving files, acknowledgements and sending retries
    public interface IFileTransferService
    {
        public void SendFile(string userName, string filePath);
        public File GetFileById(int message);
    }
}