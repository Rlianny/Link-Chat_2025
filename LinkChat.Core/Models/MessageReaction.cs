namespace LinkChat.Core.Models;

public class MessageReaction : ChatMessage
{
    public int MessageId { get { return messageId; } private set { } }
    public Emoji Reaction { get { return reaction; } private set { } }
    int messageId;
    Emoji reaction;
    public MessageReaction(string userName, DateTime timeStamp, int messageId, Emoji reaction) : base(userName, timeStamp)
    {
        (this.messageId, this.reaction) = (messageId, reaction);
    }
}
