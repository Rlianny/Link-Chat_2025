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
    private readonly IProtocolService _protocolService;
    private readonly IUserService _userService;
    private readonly IFileTransferService _fileTransferService;
    private readonly IMessagingService _messagingService;

    public AppManager(INetworkService networkService, IProtocolService protocolService, IUserService userService,
        IFileTransferService fileTransferService, IMessagingService messagingService)
    {
        _networkService = networkService;
        _protocolService = protocolService;
        _userService = userService;
        _fileTransferService = fileTransferService;
        _messagingService = messagingService;

        _messagingService.TextMessageExchanged += OnTextMessageExchanged;
        _messagingService.FileTransferred += OnFileTransfered;
        _messagingService.ChatMessageConfirmed += OnChatMessageConfirmed;
        _messagingService.ReactedToMessage += OnReactedToMessage;
        _messagingService.UserPruned += OnUserPruned; 
        //_messagingService.NewUserDetected += OnNewUserDetected;
    }
    
    public EventHandler<ChatMessage> TextMessageExchanged;
    public EventHandler<ChatMessage> FileTransferred;
    public EventHandler<ChatMessage> ChatMessageConfirmed;
    public EventHandler<ChatMessage> ReactedToMessage;
    public EventHandler<User> UserPruned;
    public EventHandler<User> NewUserDetected;
    
    
    // Events response
    public void OnTextMessageExchanged(ChatMessage message)
    {
        TextMessageExchanged?.Invoke(this, message);
    }

    public void OnFileTransfered(File file)
    {
        FileTransferred?.Invoke(this, file);
    }

    public void OnChatMessageConfirmed(ChatMessage message)
    {
        ChatMessageConfirmed?.Invoke(this, message);
    }

    public void OnReactedToMessage(ChatMessage message)
    {
        ReactedToMessage.Invoke(this, message);
    }

    public void OnUserPruned(User user)
    {
        UserPruned?.Invoke(this, user);
    }
    public void OnNewUserDetected (User user)
    {
        NewUserDetected?.Invoke(this, user);
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
        await _messagingService.SendTextMessage(userName, text);
    }

    public async Task SendFileMessage(string filePath, string userName)
    { 
        _fileTransferService.SendFile(filePath, userName);
    }

    public async Task SendReaction(string reaction, string messageId)
    {
        Emoji emoji;
        switch (reaction)
        {
            case "happy": emoji = Emoji.HappyFace; break;
            case "like": emoji = Emoji.Like; break;
            case "dislike": emoji = Emoji.Dislike; break;
            case "angry": emoji = Emoji.AngryFace; break;
            case "sad": emoji = Emoji.SadFace; break;
            case  "heart": emoji = Emoji.Heart; break;
            default: emoji =  Emoji.None; break;
        }
        
        ;
        _messagingService.ReactToMessage(messageId, emoji);
    }

    public async Task SendBroadcast(string message)
    {
        _messagingService.SendBroadcastTextMessage(message);
    }

    public async Task SetSelfUserData(string userName, string gender)
    {
        // pending implementation;
    }

    public async Task StartListening()
    {
        _networkService.StartListening();
    }

    public async Task StartUsersUpdate()
    {
        _userService.UpdateUsersStatuses();
    }
    
    
}