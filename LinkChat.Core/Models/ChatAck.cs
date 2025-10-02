namespace LinkChat.Core.Models;

public class ChatAck : Message
{
    public string MessageId { get { return messageId; } private set { } }

    string messageId;

    public ChatAck(string userName, DateTime timeStamp, string messageId) : base(userName, timeStamp)
    {
        this.messageId = messageId;
    }
}
