using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public class File : ChatMessage
{
    public string Path { get { return path; } private set { } }
    public string Name { get { return name; } private set { } }
    public double Size { get { return size; } private set { } }

    string path;
    string name;
    double size;
    public File(string userName, DateTime timeStamp, string messageId, string path, double size, string name) : base(userName, timeStamp, messageId)
    {
        (this.path, this.name, this.size) = (path, name, size);
    }
}
