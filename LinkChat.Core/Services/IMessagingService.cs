namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // manages sending and receiving messages, acknowledgements and reactions to messages
    public interface IMessagingService
    {
        public void SendChatMessage(string userName, string content);
        public TextMessage GetTextMessageById(int textMessageId);
        public IEnumerable<ChatMessage> GetChatHistory();
    }
}