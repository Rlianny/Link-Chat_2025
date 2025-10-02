using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public class TextMessage : ChatMessage
{
    public string Content { get { return content; } private set { } }
    string content;
    public TextMessage(string userName, DateTime timeStamp, int messageId, string content) : base(userName, timeStamp, messageId)
    {
        this.content = content;
    }
}
