namespace LinkChat.Core.Models
{
    public class TextMessage : ChatMessage
    {
        int messageId;
        string content;
        public TextMessage(string name, DateTime dateTime, int messageId, string content) : base(name, dateTime)
        {
            (this.messageId, this.content) = (messageId, content);
        }
    }
}