namespace LinkChat.Core.Models
{
    public abstract class Message
    {
        protected DateTime timestamp;
        protected string userName;

        public Message(string name, DateTime dateTime)
        {
            (this.userName, this.timestamp) = (name, dateTime);
        }
    }
}

