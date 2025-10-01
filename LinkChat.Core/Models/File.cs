using LinkChat;

public class File : ChatMessage
{
    public string Path { get { return path; } private set { } }
    public string Name { get { return name; } private set { } }
    public int Size { get { return size; } private set { } }
    string path;
    string name;
    int size;

    public File(string name, DateTime dateTime, string path, int size, string fileName) : base(name, dateTime)
    {
        (this.path, this.name, this.size) = (path, fileName, size);
    }
}