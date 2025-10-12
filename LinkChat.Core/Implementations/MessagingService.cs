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
    private Dictionary<string, List<Models.File>> Files = [];
    private Dictionary<string, bool> Confirmations = [];

    // Actions for UI Notifications
    public event Action<Models.File>? FileTransferred;
    public event Action<TextMessage>? TextMessageExchanged;
    public event Action<ChatMessage>? ChatMessageConfirmed;
    public event Action<ChatMessage>? ReactedToMessage;
    public event Action<User>? UserPruned;
    public event Action<string>? ErrorFounded;
    public event Action<User>? NewUserDetected;
    public event Action<UserStatus> UserIsTyping;

    public MessagingService(IProtocolService protocolService, IFileTransferService fileTransferService, IUserService userService, INetworkService networkService)
    {
        Console.WriteLine("The messaging service is created");
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

    private void OnFileSended(Models.File file, string receiverName)
    {
        AddChatMessage(receiverName, file);
        Confirmations.Add(file.MessageId, false);
        if (Files.ContainsKey(receiverName))
        {
            Files[receiverName].Add(file);
        }
        else
        {
            Files.Add(receiverName, [file]);
        }
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

    public ChatMessage GetMessageById(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
        {
            ErrorFounded?.Invoke("Message ID cannot be null or empty");
            throw new ArgumentNullException(nameof(messageId));
        }

        if (Messages.TryGetValue(messageId, out var message))
        {
            return message;
        }

        ErrorFounded?.Invoke($"Message with ID {messageId} not found or is not a text message");
        throw new KeyNotFoundException($"Message with ID {messageId} not found or is not a text message");
    }

    public async Task SendTextMessage(string receiverUserName, string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, Tools.Tools.GetNewId(userService), content);
        Confirmations.Add(textMessage.MessageId, false);

        try
        {
            byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), textMessage, false);
            await networkService.SendFrameAsync(frame, 2);
            //  System.Console.WriteLine($"Message sended with ID {textMessage.MessageId}");

            AddChatMessage(receiverUserName, textMessage);
            TextMessageExchanged?.Invoke(textMessage);

            //  System.Console.WriteLine($"A new Text Message EXchanged Event will be sended to backend: {textMessage.Content}");
            await Task.Delay(3000);
            Task sendAndWait = Task.Run(async () =>
            {
                while (!Confirmations[textMessage.MessageId])
                {
                    frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), textMessage, false);
                    await networkService.SendFrameAsync(frame, 2);
                    //          System.Console.WriteLine($"Retrying message with ID {textMessage.MessageId}");
                    await Task.Delay(3000);
                }
            });
        }
        catch (Exception ex)
        {
            ErrorFounded?.Invoke($"Error sending message: {ex.Message}");
            //    System.Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void SendBroadcastTextMessage(string content)
    {
        TextMessage textMessage = new TextMessage(userService.GetSelfUser().UserName, DateTime.Now, Tools.Tools.GetNewId(userService), content);
        byte[] frame = protocolService.CreateFrameToSend(null, textMessage, true);
        networkService.SendFrameAsync(frame, 2);
        TextMessageExchanged?.Invoke(textMessage);
        //  System.Console.WriteLine($"A new Text Message EXchanged Event will be sended to backend: {textMessage.Content}");
        //  System.Console.WriteLine($"Broadcast message sended with ID {textMessage.MessageId}");
    }

    public void ReactToMessage(string receiverUserName, string messageId, Emoji emoji)
    {
        ChatMessage chatMessage = GetMessageById(messageId);
        chatMessage.SetReaction(emoji);
        ReactedToMessage?.Invoke(chatMessage);
        // Console.WriteLine($"A new Text Reacted To Message Event will be sended from backend");
        MessageReaction messageReaction = new MessageReaction(userService.GetSelfUser().UserName, DateTime.Now, messageId, emoji);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), messageReaction, false);
        networkService.SendFrameAsync(frame, 1);
    }

    private async void OnChatAckFrameReceived(ChatAck chatAck)
    {
        if (Confirmations.ContainsKey(chatAck.MessageId))
        {
            Confirmations[chatAck.MessageId] = true;
            ChatMessageConfirmed?.Invoke(GetMessageById(chatAck.MessageId));
            //     System.Console.WriteLine($"A new Chat Message Confirmed Event will be sended to backend");
            //     System.Console.WriteLine($"Confirmation for message with ID {chatAck.MessageId} received");
        }
    }

    private async void OnTextMessageFrameReceived(TextMessage textMessage)
    {
        // Console.WriteLine("received: " + textMessage.Content);
        AddChatMessage(textMessage.UserName, textMessage);
        TextMessageExchanged?.Invoke(textMessage);
        // System.Console.WriteLine($"A new Text Message EXchanged Event will be sended to backend: {textMessage.Content}");
        // Console.WriteLine($"{textMessage.UserName}:{textMessage.Content}");
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
        //  System.Console.WriteLine($"A new Chat Message Confirmed Event will be sended to backend");
        // System.Console.WriteLine($"Confirmation sended to message with ID {chatMessage.MessageId}");
    }
    private async void OnFileMessageFrameReceived(Models.File file)
    {
        AddChatMessage(file.UserName, file);
        if (Files.ContainsKey(file.MessageId))
        {
            ErrorFounded?.Invoke($"Already exist a message with ID {file.MessageId}");
            throw new Exception($"Already exist a message with ID {file.MessageId}");
        }
        if (Files.ContainsKey(file.UserName))
        {
            Files[file.UserName].Add(file);
        }
        else
        {
            Files.Add(file.UserName, [file]);
        }
        try
        {
            await SendConfirmation(file);
        }
        catch (Exception e)
        {
            try
            {
                await Task.Delay(11000);
                await SendConfirmation(file);
            }
            catch { }
        }
        FileTransferred?.Invoke(file);
        //  System.Console.WriteLine($"A new File Transferred Event will be sended to backend: {file.Name}");
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
        //  Console.WriteLine($"A new Text Reacted To Message Event will be sended from backend RECEIVED");
    }
    private void OnUserStatusFrameReceived(UserStatus userStatus)
    {
        UserIsTyping.Invoke(userStatus);
    }

    public IEnumerable<Models.File> GetFilesHistory(string userName)
    {
        if (Files.ContainsKey(userName))
        {
            foreach (var file in Files[userName])
            {
                yield return file;
            }
        }
    }

    public void SendUserStatusTyping(string receiverUserName)
    {
        UserStatus status = new UserStatus(userService.GetSelfUser().UserName, DateTime.Now);
        byte[] frame = protocolService.CreateFrameToSend(userService.GetUserByName(receiverUserName), status, false);
        networkService.SendFrameAsync(frame, 3);
    }
}