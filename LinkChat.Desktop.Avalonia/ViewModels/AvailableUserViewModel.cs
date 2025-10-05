using System.Linq;
using LinkChat.Core.Models;
using LinkChat.Core.Services;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Implementations;
using LinkChat.Core.Interfaces;

public class AvailableUserViewModel: ViewModelBase
{
    private IMessagingService _messagingService;
    
    private string _username;
    public string Username
    {
        get { return _username; }  
    }
    
    public string LastMessage
    {
        get { return GetLastMessageContent(); }
    }
  
    public string LastTimestamp
    {
        get { return _messagingService.GetChatHistory(_username).Last().TimeStamp.ToString("HH:mm");; }
    }

    public AvailableUserViewModel(string username, IMessagingService  messagingService)
    {
        _username = username;
        _messagingService = messagingService;
    }

    private string GetLastMessageContent()
    {
        string lastMessageToReturn = "";
        ChatMessage chatMessage = _messagingService.GetChatHistory(_username).Last();
        if (chatMessage is TextMessage textMessage)
        {
            lastMessageToReturn = textMessage.Content;
        }
        else if (chatMessage is File file)
        {
            lastMessageToReturn = file.Name;
        }

        return lastMessageToReturn;
    }
}