<<<<<<< HEAD
namespace LinkChat
{

    public class File : ChatMessage, IReactable
=======
namespace LinkChat.Core.Models
{
    public class File : ChatMessage
>>>>>>> 9335ccf1a0574365d5b5dcd4ddcdfe93833f94d2
    {
        public string Path { get { return path; } private set { } }
        public string Name { get { return name; } private set { } }
        public int Size { get { return size; } private set { } }
<<<<<<< HEAD
        public Emoji Reaction => reaction;

        string path;
        string name;
        int size;
        Emoji reaction;
=======
        string path;
        string name;
        int size;
>>>>>>> 9335ccf1a0574365d5b5dcd4ddcdfe93833f94d2

        public File(string name, DateTime dateTime, string path, int size, string fileName) : base(name, dateTime)
        {
            (this.path, this.name, this.size) = (path, fileName, size);
        }
<<<<<<< HEAD

        public void SetReaction(Emoji emoji)
        {
            reaction = emoji;
        }
=======
>>>>>>> 9335ccf1a0574365d5b5dcd4ddcdfe93833f94d2
    }
}
