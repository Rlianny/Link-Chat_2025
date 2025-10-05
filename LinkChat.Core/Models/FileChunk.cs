namespace LinkChat.Core.Models;

public class FileChunk : Message
{
    public string FileId { get { return fileId; } private set { } }
    public int ChunkNumber { get { return chunkNumber; } private set { } }
    public byte[] Data { get { return data; } private set { } }
    private string fileId;
    private int chunkNumber;
    private byte[] data;

    public FileChunk(string userName, DateTime timeStamp, string fileId, int chunkNumber, byte[] data) : base(userName, timeStamp)
    {
        (this.fileId, this.chunkNumber, this.data) = (fileId, chunkNumber, data);
    }
}
