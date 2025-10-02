namespace LinkChat.Core.Models;

public class MessageReaction : Message
{
    public string MessageId { get { return messageId; } private set { } }
    public Emoji Reaction { get { return reaction; } private set { } }
    string messageId;
    Emoji reaction;
    public MessageReaction(string userName, DateTime timeStamp, string messageId, Emoji reaction) : base(userName, timeStamp)
    {
        (this.messageId, this.reaction) = (messageId, reaction);
    }
}
