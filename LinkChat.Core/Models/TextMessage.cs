namespace LinkChat
{
    public class TextMessage : ChatMessage, IReactable
    {
        public int MessageId { get { return messageId; } private set { } }
        public string Content { get { return content; } private set { } }
        public Emoji Reaction { get { return reaction; } private set { } }

        int messageId;
        string content;
        Emoji reaction;
        public TextMessage(string name, DateTime dateTime, int messageId, string content) : base(name, dateTime)
        {
            (this.messageId, this.content) = (messageId, content);
        }


        public void SetReaction(Emoji emoji)
        {
            reaction = emoji;
        }
    }
}