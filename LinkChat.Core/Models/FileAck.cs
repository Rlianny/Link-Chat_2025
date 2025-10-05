namespace LinkChat.Core.Models;

public class FileAck : Message
{
    public string FileID { get { return fileId; } private set { } }
    public int ChunkNumber { get { return chunkNumber; } private set { } }
    string fileId;
    int chunkNumber;

    public FileAck(string userName, DateTime timeStamp, string fileId, int chunkNumber) : base(userName, timeStamp)
    {
        (this.fileId, this.chunkNumber) = (fileId, chunkNumber);
    }
}
