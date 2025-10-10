using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkChat.Core.Models;
using LinkChat.Core.Services;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Implementations;
using LinkChat.Core.Interfaces;

public partial class AvailableUserViewModel: ViewModelBase
{
    private AppManager appManager;
    
    private User _user;

    public User User
    {
        get => _user;
    }

    public string UserName
    {
        get => _user.UserName;
    }
    
    [ObservableProperty] private string _lastMessage;
    [ObservableProperty] private string _lastTimestamp;

    public AvailableUserViewModel(User user, AppManager appManager)
    {
        _user = user;
        _lastMessage = " ";
        _lastTimestamp = " ";
        this.appManager = appManager;

        appManager.TextMessageExchanged += OnTextMessageExchanged;
        appManager.UserPruned += OnUserPruned;

    }

    public void OnTextMessageExchanged(object? sender, ChatMessage chatMessage)
    {
        if(chatMessage.UserName == _user.UserName || chatMessage.UserName == appManager.GetCurrentSelfUser().UserName)
        Update();
    }

    private void Update()
    {
        ChatMessage chatMessage = appManager.GetLastMessageByUser(_user.UserName);
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
        if (user.UserName == UserName)
            Dispose();
    }

    private void Dispose()
    {
        appManager.TextMessageExchanged -= OnTextMessageExchanged;
        appManager.UserPruned -= OnUserPruned;
    }

    [RelayCommand]
    public void ReloadChat()
    {
        GlobalSingletonHelper.ChatWindowViewModel.ReloadChat(UserName);
    }
    
}