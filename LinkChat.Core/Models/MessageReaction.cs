namespace LinkChat
{
    public class MessageReaction : ChatMessage
    {
        public int MessageId { get { return messageId; } private set { } }
        public Emoji Reaction { get { return reaction; } private set { } }
        int messageId;
        Emoji reaction;
        public MessageReaction(string name, DateTime dateTime, int messageId, Emoji emoji) : base(name, dateTime)
        {
            (this.messageId, reaction) = (messageId, emoji);
        }
    }
}