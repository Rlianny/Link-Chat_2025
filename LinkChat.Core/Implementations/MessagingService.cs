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
    private Dictionary<string, List<ChatMessage>> Conversation = new();
    private Dictionary<string, ChatMessage> Messages = [];
    private Dictionary<string, Models.File> Files = [];
    private Dictionary<string, bool> Confirmations = [];

    // Actions for UI Notifications
    public event Action<Models.File>? FileTransferred;
    public event Action<TextMessage>? TextMessageExchanged;
    public event Action<ChatMessage>? ChatMessageConfirmed;
    public event Action<ChatMessage>? ReactedToMessage;
    public event Action<User>? UserPruned;
    public event Action<string>? ErrorFounded;
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
        fileTransferService.FileSended += OnFileSended;

    }

    private void OnFileSended(Models.File file)
    {
        AddChatMessage(userService.GetSelfUser().UserName, file);
        FileTransferred?.Invoke(file);
    }

    public IEnumerable<ChatMessage> GetChatHistory(string userName)
    {
        if (Conversation.ContainsKey(userName))
        {
            return Conversation[userName].ToList();
        }
        return new List<ChatMessage>();
    }
    private void AddChatMessage(string userSender, ChatMessage chatMessage)
    {
        if (!Messages.ContainsKey(chatMessage.MessageId))
        {
            Messages.Add(chatMessage.MessageId, chatMessage);
        }
        if (!Conversation.ContainsKey(userSender))
        {
            Conversation.Add(userSender, []);
        }

        Conversation[userSender].Add(chatMessage);
    }

    public TextMessage GetTextMessageById(string textMessageId)
    {
        if (string.IsNullOrEmpty(textMessageId))
        {
            ErrorFounded?.Invoke("Message ID cannot be null or empty");
            throw new ArgumentNullException(nameof(textMessageId));
        }

        if (Messages.TryGetValue(textMessageId, out var message) && message is TextMessage text)
        {
            return text;
        }

        ErrorFounded?.Invoke($"Message with ID {textMessageId} not found or is not a text message");
        throw new KeyNotFoundException($"Message with ID {textMessageId} not found or is not a text message");
    }

    public async Task SendTextMessage(string receiverUserName, string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, Tools.Tools.GetNewId(userService), content);
        Confirmations.Add(textMessage.MessageId, false);

        try
        {
            byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), textMessage, false);
            await networkService.SendFrameAsync(frame, 2);
            System.Console.WriteLine($"Message sended with ID {textMessage.MessageId}");

            AddChatMessage(receiverUserName, textMessage);
            TextMessageExchanged?.Invoke(textMessage);

            System.Console.WriteLine($"A new Text Message EXchanged Event will be sended to backend: {textMessage.Content}");
            await Task.Delay(3000);
            Task sendAndWait = Task.Run(async () =>
            {
                while (!Confirmations[textMessage.MessageId])
                {
                    frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), textMessage, false);
                    await networkService.SendFrameAsync(frame, 2);
                    System.Console.WriteLine($"Retrying message with ID {textMessage.MessageId}");
                    await Task.Delay(3000);
                }
            });
        }
        catch (Exception ex)
        {
            ErrorFounded?.Invoke($"Error sending message: {ex.Message}");
            System.Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void SendBroadcastTextMessage(string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, Tools.Tools.GetNewId(userService), content);
        byte[] frame = protocolService.CreateFrameToSend(null, textMessage, true);
        networkService.SendFrameAsync(frame, 2);
        TextMessageExchanged?.Invoke(textMessage);
        System.Console.WriteLine($"A new Text Message EXchanged Event will be sended to backend: {textMessage.Content}");
        System.Console.WriteLine($"Broadcast message sended with ID {textMessage.MessageId}");
    }

    public void ReactToMessage(string messageId, Emoji emoji)
    {
        MessageReaction messageReaction = new MessageReaction(userService.GetSelfUser().UserName, DateTime.Now, messageId, emoji);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(GetTextMessageById(messageId).UserName), messageReaction, false);
        networkService.SendFrameAsync(frame, 3);
        ReactedToMessage?.Invoke(GetTextMessageById(messageId));
        System.Console.WriteLine($"A new Text Reacted To Message Event will be sended to backend");
    }

    private async void OnChatAckFrameReceived(ChatAck chatAck)
    {
        if (Confirmations.ContainsKey(chatAck.MessageId))
        {
            Confirmations[chatAck.MessageId] = true;
            ChatMessageConfirmed?.Invoke(GetTextMessageById(chatAck.MessageId));
            System.Console.WriteLine($"A new Chat Message Confirmed Event will be sended to backend");
            System.Console.WriteLine($"Confirmation for message with ID {chatAck.MessageId} received");
        }
    }

    private async void OnTextMessageFrameReceived(TextMessage textMessage)
    {
        Console.WriteLine("received: " + textMessage.Content);
        AddChatMessage(textMessage.UserName, textMessage);
        TextMessageExchanged?.Invoke(textMessage);
        System.Console.WriteLine($"A new Text Message EXchanged Event will be sended to backend: {textMessage.Content}");
        Console.WriteLine($"{textMessage.UserName}:{textMessage.Content}");
        try
        {
            await SendConfirmation(textMessage);
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
        await networkService.SendFrameAsync(frame, 1);
        ChatMessageConfirmed?.Invoke(chatMessage);
        System.Console.WriteLine($"A new Chat Message Confirmed Event will be sended to backend");
        System.Console.WriteLine($"Confirmation sended to message with ID {chatMessage.MessageId}");
    }
    private void OnFileMessageFrameReceived(Models.File file)
    {
        AddChatMessage(file.UserName, file);
        if (Files.ContainsKey(file.MessageId))
        {
            ErrorFounded?.Invoke($"Already exist a message with ID {file.MessageId}");
            throw new Exception($"Already exist a message with ID {file.MessageId}");
        }
        Files.Add(file.MessageId, file);
        FileTransferred?.Invoke(file);
        System.Console.WriteLine($"A new File Transferred Event will be sended to backend: {file.Name}");
    }
    private void OnMessageReactionFrameReceived(MessageReaction reaction)
    {
        if (!Messages.ContainsKey(reaction.MessageId))
        {
            ErrorFounded?.Invoke($"Message with ID {reaction.MessageId} doesn't exist");
            throw new Exception($"Message with ID {reaction.MessageId} doesn't exist");
        }
        Messages[reaction.MessageId].SetReaction(reaction.Reaction);
        ReactedToMessage?.Invoke(Messages[reaction.MessageId]);
        System.Console.WriteLine($"A new Text Reacted To Message Event will be sended to backend");
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