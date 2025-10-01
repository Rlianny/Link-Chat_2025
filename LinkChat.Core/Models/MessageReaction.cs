class MessageReaction : Message
{
    int messageId;
    Emoji reaction;
    public MessageReaction(string name, DateTime dateTime, int messageId, Emoji emoji) : base(name, dateTime)
    {
        (this.messageId, reaction) = (messageId, emoji);
    }
}