namespace LinkChat.Core.Models;

public class ChatAck : Message
{
    public int MessageId { get { return messageId; } private set { } }

    int messageId;

    public ChatAck(string userName, DateTime timeStamp, int messageId) : base(userName, timeStamp)
    {
        this.messageId = messageId;
    }
}
