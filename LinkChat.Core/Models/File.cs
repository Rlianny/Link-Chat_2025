namespace LinkChat.Core.Models
{

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
        public File(string name, DateTime dateTime, string path, int size, string fileName) : base(name, dateTime)
        {
            (this.path, this.name, this.size) = (path, fileName, size);
        }

        public void SetReaction(Emoji emoji)
        {
            reaction = emoji;
        }
    }
}
