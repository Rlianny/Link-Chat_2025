using LinkChat.Core.Implementations;
using LinkChat.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkChat.Core.Models;
using LinkChat.Core.Services;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public class AppManager
{
    private readonly INetworkService _networkService;
    private readonly IUserService _userService;
    private readonly IFileTransferService _fileTransferService;
    private readonly IMessagingService _messagingService;

    public AppManager(INetworkService networkService, IProtocolService protocolService, IUserService userService,
        IFileTransferService fileTransferService, IMessagingService messagingService)
    {
        _networkService = networkService;
        _userService = userService;
        _fileTransferService = fileTransferService;
        _messagingService = messagingService;

        _messagingService.TextMessageExchanged += OnTextMessageExchanged;
        _messagingService.FileTransferred += OnFileTransfered;
        _messagingService.ChatMessageConfirmed += OnChatMessageConfirmed;
        _messagingService.ReactedToMessage += OnReactedToMessage;
        _messagingService.UserIsTyping += OnUserIsTyping;
        _userService.UserDisconnected += OnUserPruned; 
        _userService.NewUserConnected += OnNewUserDetected;
        //USER STATUS UPDATING PENDING
    }

    public EventHandler<ChatMessage> TextMessageExchanged;
    public EventHandler<ChatMessage> FileTransferred;
    public EventHandler<ChatMessage> ChatMessageConfirmed;
    public EventHandler<ChatMessage> ReactedToMessage;
    public EventHandler<User> UserPruned;
    public EventHandler<User> NewUserDetected;
    public EventHandler<User> UserStatusUpdated;
    
    
    // Events response
    private void OnTextMessageExchanged(ChatMessage message)
    {
        TextMessageExchanged?.Invoke(this, message);
    }

    private void OnFileTransfered(File file)
    {
        FileTransferred?.Invoke(this, file);
    }

    private void OnChatMessageConfirmed(ChatMessage message)
    {
        ChatMessageConfirmed?.Invoke(this, message);
    }

    private void OnReactedToMessage(ChatMessage message)
    {
        ReactedToMessage.Invoke(this, message);
    }

    private void OnUserPruned(User user)
    {
        UserPruned?.Invoke(this, user);
    }
    private void OnNewUserDetected (User user)
    {
        NewUserDetected?.Invoke(this, user);
    }

    private void OnUserStatusUpdated(User user)
    {
        UserStatusUpdated.Invoke(this, user);
    }
    
    private void OnUserIsTyping(UserStatus obj)
    {
        UserStatusUpdated.Invoke(this, _userService.GetUserByName(obj.UserName));
    }

    // Get Info from Backend
    public User GetCurrentSelfUser()
    {
        return _userService.GetSelfUser();
    }

    public User GetUserByName(string userName)
    {
        return _userService.GetUserByName(userName);
    }

    public List<ChatMessage> GetChatHistory(string userName)
    {
        return (List<ChatMessage>)_messagingService.GetChatHistory(userName);
    }

    public List<File> GetFilesHistory(string userName)
    {
        // pending implementation
        return null;
    }

    public ChatMessage GetLastMessageByUser(string userName)
    {
        return _messagingService.GetChatHistory(userName).Last();
    }

    // Send Info to Backend
    public async Task SendTextMessage(string text, string userName)
    {
        _messagingService.SendTextMessage(userName, text);
        Console.WriteLine("Frontend: A Text Message Sent");
    }

    public async Task SendFileMessage(string filePath, string userName)
    { 
        _fileTransferService.SendFile(userName, filePath);
        Console.WriteLine("Frontend: A File Message Sent");
    }

    public async Task SendReaction(Emoji emoji, string messageId)
    {
        _messagingService.ReactToMessage(messageId, emoji);
        Console.WriteLine("Frontend: A Reaction Sent");
    }

    public async Task SendBroadcast(string message)
    {
        _messagingService.SendBroadcastTextMessage(message);
        Console.WriteLine("Frontend: A Broadcast Sent");
    }

    public async Task SendUserStatusTyping()
    {
        Console.WriteLine("Frontend: Sending user status typing...");
    }

    public async Task SetSelfUserData(string userName, string gender)
    {
        // pending implementation;
    }

    public async Task StartListening()
    {
        _networkService.StartListening();
        Console.WriteLine("Frontend: Listening started");
    }
    
}