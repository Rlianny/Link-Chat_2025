namespace LinkChat.Core.Models
{
    public abstract class Message
    {
        public DateTime TimeStamp { get { return timestamp; } private set { } }
        public string UserName { get { return userName; } private set { } }
        protected DateTime timestamp;
        protected string userName;

        public Message(string name, DateTime dateTime)
        {
            (this.userName, this.timestamp) = (name, dateTime);
        }
    }
}

