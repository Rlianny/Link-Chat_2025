namespace LinkChat
{
    public class ChatAck : Message
    {
        public int MessageId { get { return messageId; } private set { } }

        int messageId;

        public ChatAck(string name, DateTime dateTime, int messageId) : base(name, dateTime)
        {
            this.messageId = messageId;
        }
    }
}