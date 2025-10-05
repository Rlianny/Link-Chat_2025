namespace LinkChat.Core.Models;

public class FileStart : Message
{
    public string FileName { get { return fileName; } private set { } }
    public double FileSize { get { return fileSize; } private set { } }
    public string FileId { get { return fileId; } private set { } }
    public int TotalChunks { get { return totalChunks; } private set { } }
    string fileName;
    double fileSize;
    string fileId;
    int totalChunks;
    public FileStart(string userName, DateTime timeStamp, string fileName, double fileSize, string fileId, int totalChunks) : base(userName, timeStamp)
    {
        (this.fileName, this.fileSize, this.fileId, this.totalChunks) = (fileName, fileSize, fileId, totalChunks);
    }
}