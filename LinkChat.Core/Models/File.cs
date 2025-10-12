using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public class File : ChatMessage
{
    public string Path { get { return path; } private set { } }
    public string Name { get { return name; } private set { } }
    public long Size { get { return size; } private set { } }

    string path;
    string name;
    long size;
    public File(string userName, DateTime timeStamp, string messageId, string path, long size, string name) : base(userName, timeStamp, messageId)
    {
        (this.path, this.name, this.size) = (path, name, size);
    }
}
