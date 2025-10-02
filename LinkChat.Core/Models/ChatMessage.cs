namespace LinkChat.Core.Models
{
    public abstract class ChatMessage : Message
    {
        public ChatMessage(string name, DateTime dateTime) : base(name, dateTime)
        { }
    }
}