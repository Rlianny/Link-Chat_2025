public class FileAck : Message
{
    int fileId;
    int chunkNumber;

    public FileAck(string name, DateTime dateTime, int fileId, int chunkNumber) : base(name, dateTime)
    {
        (this.fileId, this.chunkNumber) = (fileId, chunkNumber);
    }
}