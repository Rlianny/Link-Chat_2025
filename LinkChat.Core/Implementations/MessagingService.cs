using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class MessagingService : IMessagingService
{
    private Dictionary<string, List<ChatMessage>> Conversation = [];
    private Dictionary<string, ChatMessage> Messages = [];
    private Dictionary<string, Models.File> Files = [];


    public MessagingService(IProtocolService protocolService, IFileTransferService fileTransferService)
    {
        protocolService.ChatAckFrameReceived += OnChatAckFrameReceived;
        protocolService.TextMessageFrameReceived += OnTextMessageFrameReceived;
        protocolService.MessageReactionFrameReceived += OnMessageReactionFrameReceived;
        protocolService.UserStatusFrameReceived += OnUserStatusFrameReceived;
        fileTransferService.FileFrameReceived += OnFileMessageFrameReceived;
    }
    public IEnumerable<ChatMessage> GetChatHistory(string userName)
    {
        if (Conversation.ContainsKey(userName))
        {
            return Conversation[userName];
        }
        return [];
    }
    private void AddChatMessage(ChatMessage chatMessage)
    {
        if (Messages.ContainsKey(chatMessage.MessageId))
        {
            throw new Exception($"Already exist a ChatMessage with ID {chatMessage.MessageId}");
        }
        Messages.Add(chatMessage.MessageId, chatMessage);
        if (!Conversation.ContainsKey(chatMessage.UserName))
        {
            Conversation.Add(chatMessage.UserName, []);
        }
        Conversation[chatMessage.UserName].Add(chatMessage);
    }

    public TextMessage GetTextMessageById(string textMessageId)
    {
        if (Messages.ContainsKey(textMessageId) && Messages[textMessageId] is TextMessage text)
        {
            return text;
        }
        throw new Exception($"Doesn't exist a message with ID {textMessageId}");
    }

    public void SendChatMessage(string userName, string content)
    {
        throw new NotImplementedException();
    }
    private void OnChatAckFrameReceived(ChatAck chatAck)
    {

    }
    private void OnTextMessageFrameReceived(TextMessage textMessage)
    {
        AddChatMessage(textMessage);
    }
    private void OnFileMessageFrameReceived(Models.File file)
    {
        AddChatMessage(file);
        if (Files.ContainsKey(file.MessageId))
        {
            throw new Exception($"Already exist a message with ID {file.MessageId}");
        }
        Files.Add(file.MessageId, file);
    }
    private void OnMessageReactionFrameReceived(MessageReaction reaction)
    {
        if (!Messages.ContainsKey(reaction.MessageId))
        {
            throw new Exception($"Message with ID {reaction.MessageId} doesn't exist");
        }
        Messages[reaction.MessageId].SetReaction(reaction.Reaction);
    }
    private void OnUserStatusFrameReceived(UserStatus status)
    {

    }
}