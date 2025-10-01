namespace LinkChat.Core
{
    public class ChatAck : Message
    {
        int messageId;

        public ChatAck(string name, DateTime dateTime, int messageId) : base(name, dateTime)
        {
            this.messageId = messageId;
        }
    }
}