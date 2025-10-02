namespace LinkChat.Core.Models
{
    public class FileAck : Message
    {
        public int FileID { get { return fileId; } private set { } }
        public int ChunkNumber { get { return chunkNumber; } private set { } }
        int fileId;
        int chunkNumber;

        public FileAck(string name, DateTime dateTime, int fileId, int chunkNumber) : base(name, dateTime)
        {
            (this.fileId, this.chunkNumber) = (fileId, chunkNumber);
        }
    }
}