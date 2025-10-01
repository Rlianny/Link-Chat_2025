namespace LinkChat
{
    public abstract class ChatMessage : Message
    {
        public ChatMessage(string name, DateTime dateTime) : base(name, dateTime)
        { }
    }
}