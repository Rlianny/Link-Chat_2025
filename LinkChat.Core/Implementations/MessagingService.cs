using System.Threading.Tasks;
using LinkChat.Core.Models;
using LinkChat.Core.Tools;
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

    // Actions for UI Notifications
    public event Action<Models.File>? FileTransferred;
    public event Action<TextMessage>? TextMessageExchanged;
    public event Action<ChatMessage>? ChatMessageConfirmed;
    public event Action<ChatMessage>? ReactedToMessage;
    public event Action<User> UserPruned;
    public event Action<string> ErrorFounded;
    public event Action<User>? NewUserDetected;

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

        ErrorFounded.Invoke($"Doesn't exist a message with ID {textMessageId}");
        throw new Exception($"Doesn't exist a message with ID {textMessageId}");
    }

    public async Task SendTextMessage(string receiverUserName, string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, Tools.Tools.GetNewId(userService), content);
        Confirmations.Add(textMessage.MessageId, false);
        Task sendAndWait = Task.Run(async () =>
        {
            while (!Confirmations[textMessage.MessageId])
            {
                byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), textMessage, false);
                await networkService.SendFrameAsync(frame);
                System.Console.WriteLine($"Message sended with ID {textMessage.MessageId}");
                await Task.Delay(3000);
                TextMessageExchanged.Invoke(textMessage);
            }
        });

        await sendAndWait;
    }

    public void SendBroadcastTextMessage(string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, Tools.Tools.GetNewId(userService), content);
        byte[] frame = protocolService.CreateFrameToSend(null, textMessage, true);
        networkService.SendFrameAsync(frame);
        TextMessageExchanged.Invoke(textMessage);
        System.Console.WriteLine($"Broadcast message sended with ID {textMessage.MessageId}");

        //pending to decide acks or not
    }

    public void ReactToMessage(string messageId, Emoji emoji)
    {
        MessageReaction messageReaction = new MessageReaction(userService.GetSelfUser().UserName, DateTime.Now, messageId, emoji);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(GetTextMessageById(messageId).UserName), messageReaction, false);
        networkService.SendFrameAsync(frame);
        ReactedToMessage.Invoke(GetTextMessageById(messageId));
    }

    private void OnChatAckFrameReceived(ChatAck chatAck)
    {
        if (Confirmations.ContainsKey(chatAck.MessageId))
        {
            Confirmations[chatAck.MessageId] = true;
            ChatMessageConfirmed.Invoke(Messages[chatAck.MessageId]);
            System.Console.WriteLine($"Confirmation for message with ID {chatAck.MessageId} received");
        }
    }

    private async void OnTextMessageFrameReceived(TextMessage textMessage)
    {
        AddChatMessage(textMessage);
        TextMessageExchanged.Invoke(textMessage);
        Console.WriteLine($"{textMessage.UserName}:{textMessage.Content}");
        try
        {
            await SendConfirmation(textMessage);
            ChatMessageConfirmed.Invoke(textMessage);
        }
        catch (Exception e)
        {
            try
            {
                await Task.Delay(11000);
                await SendConfirmation(textMessage);
            }
            catch { }
        }
    }
    private async Task SendConfirmation(ChatMessage chatMessage)
    {
        ChatAck ack = new ChatAck(chatMessage.UserName, DateTime.Now, chatMessage.MessageId);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(chatMessage.UserName), ack, false);
        await networkService.SendFrameAsync(frame);
        ChatMessageConfirmed.Invoke(chatMessage);
        System.Console.WriteLine($"Confirmation sended to message with ID {chatMessage.MessageId}");
    }
    private void OnFileMessageFrameReceived(Models.File file)
    {
        AddChatMessage(file);
        if (Files.ContainsKey(file.MessageId))
        {
            ErrorFounded.Invoke($"Already exist a message with ID {file.MessageId}");
            throw new Exception($"Already exist a message with ID {file.MessageId}");
        }
        Files.Add(file.MessageId, file);
        //FileTransferred.Invoke(file);
    }
    private void OnMessageReactionFrameReceived(MessageReaction reaction)
    {
        if (!Messages.ContainsKey(reaction.MessageId))
        {
            ErrorFounded.Invoke($"Message with ID {reaction.MessageId} doesn't exist");
            throw new Exception($"Message with ID {reaction.MessageId} doesn't exist");
        }
        Messages[reaction.MessageId].SetReaction(reaction.Reaction);
        ReactedToMessage.Invoke(Messages[reaction.MessageId]);
    }
    private void OnUserStatusFrameReceived(UserStatus status)
    {
        // pending implementation
    }

    public IEnumerable<Models.File> GetFilesHistory(string userName)
    {
        // pending implementation
        throw new NotImplementedException();
    }
}