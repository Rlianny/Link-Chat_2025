namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // manages sending and receiving messages, acknowledgements and reactions to messages
    public interface IMessagingService
    {
        public void SendTextMessage(string receiverUserName, string content);
        public void ReactToMessage(string messageId, Emoji emoji);
        public string GetNewId();
        public TextMessage GetTextMessageById(string textMessageId);
        public IEnumerable<ChatMessage> GetChatHistory(string UserName);
    }
}