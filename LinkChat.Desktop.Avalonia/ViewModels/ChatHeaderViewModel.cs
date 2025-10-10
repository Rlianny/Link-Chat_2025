using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public partial class ChatHeaderViewModel : ViewModelBase
{
    private AppManager _appManager;
    
    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _userStatus;

    public ChatHeaderViewModel(User user, AppManager appManager)
    {
        _username = user.UserName;
        _userStatus = user.Status.ToString();

        _appManager = appManager;
        _appManager.UserStatusUpdated += OnUserStatusUpdated;
        _appManager.NewUserDetected += OnNewUserDetected;
        _appManager.UserPruned += OnUserPruned;
    }

    private void OnUserPruned(object? sender, User user)
    {
        if (user.UserName == _username)
        {
            UserStatus = "Offline";
        }
    }

    private void OnNewUserDetected(object? sender, User user)
    { 
        if (user.UserName == _username)
        {
            UserStatus = "Online";
        }
    }

    private async void OnUserStatusUpdated(object? sender, User user)
    {
        if (user.UserName == _username)
        {
            UserStatus = "Typing...";
            await Task.Delay(2000);
        }
    }
}