namespace LinkChat.Core.Models
{
    public class FileChunk : Message
    {
        int fileId;
        int chunkNumber;
        byte[] data;


        public FileChunk(string name, DateTime dateTime, int fileId, int chunkNumber, byte[] data) : base(name, dateTime)
        {
            (this.fileId, this.chunkNumber, this.data) = (fileId, chunkNumber, data);
        }
    }
}