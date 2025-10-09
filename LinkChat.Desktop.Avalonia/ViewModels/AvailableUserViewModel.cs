using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;
using LinkChat.Core.Services;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Implementations;
using LinkChat.Core.Interfaces;

public partial class AvailableUserViewModel: ViewModelBase
{
    private AppManager appManager;
    
    private string _userName;

    public string UserName
    {
        get => _userName;
    }
    
    [ObservableProperty] private string _lastMessage;
    [ObservableProperty] private string _lastTimestamp;

    public AvailableUserViewModel(string username, AppManager appManager)
    {
        _userName = username;
        _lastMessage = " ";
        _lastTimestamp = " ";
        this.appManager = appManager;

        appManager.TextMessageExchanged += OnTextMessageExchanged;
        appManager.UserPruned += OnUserPruned;

    }

    public void OnTextMessageExchanged(object sender, ChatMessage chatMessage)
    {
        if(chatMessage.UserName == _userName || chatMessage.UserName == appManager.GetCurrentSelfUser().UserName)
        Update();
    }

    private void Update()
    {
        ChatMessage chatMessage = appManager.GetLastMessageByUser(this._userName);
        if (chatMessage is TextMessage textMessage)
        {
            LastMessage = textMessage.Content;
        }
        else if (chatMessage is File file)
        {
            LastMessage = file.Name;
        }

        LastTimestamp = chatMessage.TimeStamp.ToString("HH:mm");
    }

    private void OnUserPruned(object sender, User user)
    {
        if (user.UserName == _userName)
            Dispose();
    }

    private void Dispose()
    {
        appManager.TextMessageExchanged -= OnTextMessageExchanged;
        appManager.UserPruned -= OnUserPruned;
    }

    
}