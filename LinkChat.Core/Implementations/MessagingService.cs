using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class MessagingService : IMessagingService
{
    private IProtocolService protocolService;
    private IFileTransferService fileTransferService;
    private INetworkService networkService;
    private IUserService userService;
    private Dictionary<string, List<ChatMessage>> Conversation = [];
    private Dictionary<string, ChatMessage> Messages = [];
    private Dictionary<string, Models.File> Files = [];
    private Dictionary<string, bool> Confirmations = [];

    public MessagingService(IProtocolService protocolService, IFileTransferService fileTransferService, IUserService userService, INetworkService networkService)
    {
        this.protocolService = protocolService;
        this.networkService = networkService;
        this.fileTransferService = fileTransferService;
        this.userService = userService;
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
        if (!Messages.ContainsKey(chatMessage.MessageId))
        {
            Messages.Add(chatMessage.MessageId, chatMessage);
        }
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

    public void SendChatMessage(string receiverUserName, string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, GetNewId(), content);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), textMessage, false);
        networkService.SendFrameAsync(frame);
        // pending wait for ack implementation
    }

    public void ReactToMessage(string messageId, Emoji emoji)
    {
        MessageReaction messageReaction = new MessageReaction(userService.GetSelfUser().UserName, DateTime.Now, messageId, emoji);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(GetTextMessageById(messageId).UserName), messageReaction, false);
        networkService.SendFrameAsync(frame);
    }

    private void OnChatAckFrameReceived(ChatAck chatAck)
    {

        if (!Confirmations.ContainsKey(chatAck.MessageId))
        {
            throw new Exception($"Confirmation {chatAck} was missed");
        }
        Confirmations[chatAck.MessageId] = true;
    }

    private void OnTextMessageFrameReceived(TextMessage textMessage)
    {
        AddChatMessage(textMessage);
        Console.WriteLine($"{textMessage.UserName}:{textMessage.Content}");
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
        // pending implementation
    }

    public string GetNewId()
    {
        var timestamp = DateTime.UtcNow.Ticks; // 100ns precision
        var random = Random.Shared.Next(10000);
        string userName = userService.GetSelfUser().UserName;
        return $"{userName}_{timestamp}_{random:0000}";
    }

    public void SendChatMessageWithMac(byte[] mac, string content)
    {
        User receiver = new User("John Doe", Status.Online, mac);
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, GetNewId(), content);
        byte[] frame = protocolService.CreateFrameToSend(receiver, textMessage, false);
        networkService.SendFrameAsync(frame);
        // pending wait for ack implementation
    }
}