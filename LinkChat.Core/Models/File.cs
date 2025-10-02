using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public class File : ChatMessage, IReactable
{
    public string Path { get { return path; } private set { } }
    public string Name { get { return name; } private set { } }
    public int Size { get { return size; } private set { } }
    public Emoji Reaction => reaction;

    string path;
    string name;
    int size;
    Emoji reaction;
    public File(string userName, DateTime timeStamp, string path, int size, string name) : base(userName, timeStamp)
    {
        (this.path, this.name, this.size) = (path, name, size);
    }

    public void SetReaction(Emoji emoji)
    {
        reaction = emoji;
    }
}
