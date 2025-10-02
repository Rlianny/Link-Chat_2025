namespace LinkChat.Core.Models;
public class FileAck : Message
{
    public int FileID { get { return fileId; } private set { } }
    public int ChunkNumber { get { return chunkNumber; } private set { } }
    int fileId;
    int chunkNumber;

    public FileAck(string userName, DateTime timeStamp, int fileId, int chunkNumber) : base(userName, timeStamp)
    {
        (this.fileId, this.chunkNumber) = (fileId, chunkNumber);
    }
}
