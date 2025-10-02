using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public class TextMessage : ChatMessage, IReactable
{
    public int MessageId { get { return messageId; } private set { } }
    public string Content { get { return content; } private set { } }
    public Emoji Reaction { get { return reaction; } private set { } }

    int messageId;
    string content;
    Emoji reaction;
    public TextMessage(string userName, DateTime timeStamp, int messageId, string content) : base(userName, timeStamp)
    {
        (this.messageId, this.content) = (messageId, content);
    }


    public void SetReaction(Emoji emoji)
    {
        reaction = emoji;
    }
}
