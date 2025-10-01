public class ChatMessage : Message
{
    int messageId;
    string content;
    public ChatMessage(string name, DateTime dateTime, int messageId, string content) : base(name, dateTime)
    {
        (this.messageId, this.content) = (messageId, content);
    }
}