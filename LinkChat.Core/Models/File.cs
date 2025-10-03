using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public class File : ChatMessage
{
    public string Path { get { return path; } private set { } }
    public string Name { get { return name; } private set { } }
    public int Size { get { return size; } private set { } }

    string path;
    string name;
    int size;
    public File(string userName, DateTime timeStamp, string messageId, string path, int size, string name) : base(userName, timeStamp, messageId)
    {
        (this.path, this.name, this.size) = (path, name, size);
    }
}
