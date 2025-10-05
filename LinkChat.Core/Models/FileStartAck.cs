using LinkChat.Core.Models;

public class FileStartAck : Message
{
    public string FileId { get { return fileId; } private set { } }
    string fileId;
    public FileStartAck(string userName, DateTime timeStamp, string fileId) : base(userName, timeStamp)
    {
        this.fileId = fileId;
    }
}