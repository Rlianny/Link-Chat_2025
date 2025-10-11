namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;
    using System.Threading.Tasks;

    // manages sending and receiving messages, acknowledgements and reactions to messages
    public interface IMessagingService
    {
        public Task SendTextMessage(string receiverUserName, string content);
        public void SendBroadcastTextMessage(string content);
        public void ReactToMessage(string messageId, Emoji emoji);
        public ChatMessage GetMessageById(string textMessageId);
        public IEnumerable<ChatMessage> GetChatHistory(string UserName);
        public IEnumerable<File> GetFilesHistory(string userName);
        public event Action<File>? FileTransferred;
        public event Action<TextMessage>? TextMessageExchanged;
        public event Action<ChatMessage>? ChatMessageConfirmed;
        public event Action<ChatMessage>? ReactedToMessage;
        public event Action<User>? UserPruned;
        public event Action<User>? NewUserDetected;
        public event Action<string>? ErrorFounded;  
    }
}