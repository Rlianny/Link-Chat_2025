namespace LinkChat.Core.Models;

public class FileChunk : Message
{
    public int FileId { get { return fileId; } private set { } }
    public int ChunkNumber { get { return chunkNumber; } private set { } }
    public byte[] Data { get { return data; } private set { } }
    int fileId;
    int chunkNumber;
    byte[] data;


    public FileChunk(string name, DateTime dateTime, int fileId, int chunkNumber, byte[] data) : base(name, dateTime)
    {
        (this.fileId, this.chunkNumber, this.data) = (fileId, chunkNumber, data);
    }
}
