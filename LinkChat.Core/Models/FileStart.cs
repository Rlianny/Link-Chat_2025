namespace LinkChat.Core.Models
{
    public class FileStart : Message
    {
        string fileName;
        int fileSize;
        int fileId;
        int totalChunks;
        public FileStart(string name, DateTime dateTime, string fileName, int fileSize, int fileId, int totalChunks) : base(name, dateTime)
        {
            (this.fileName, this.fileSize, this.fileId, this.totalChunks) = (fileName, fileSize, fileId, totalChunks);
        }
    }
}