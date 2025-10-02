namespace LinkChat.Core.Models;

public class FileStart : Message
{
    public string FileName { get { return fileName; } private set { } }
    public int FileSize { get { return fileSize; } private set { } }
    public int FileId { get { return fileId; } private set { } }
    public int TotalChunks { get { return totalChunks; } private set { } }
    string fileName;
    int fileSize;
    int fileId;
    int totalChunks;
    public FileStart(string userName, DateTime timeStamp, string fileName, int fileSize, int fileId, int totalChunks) : base(userName, timeStamp)
    {
        (this.fileName, this.fileSize, this.fileId, this.totalChunks) = (fileName, fileSize, fileId, totalChunks);
    }
}