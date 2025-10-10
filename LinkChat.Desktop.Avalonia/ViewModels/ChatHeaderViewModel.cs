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

    [ObservableProperty]
    private User? _currentReceiverUser;

    public ChatHeaderViewModel(User user, AppManager appManager)
    {
        CurrentReceiverUser = user;
        Username = CurrentReceiverUser.UserName;
        UserStatus = CurrentReceiverUser.Status.ToString();

        _appManager = appManager;
        _appManager.UserStatusUpdated += OnUserStatusUpdated;
        _appManager.NewUserDetected += OnNewUserDetected;
        _appManager.UserPruned += OnUserPruned;
    }

    private void OnUserPruned(object? sender, User user)
    {
        if (user.UserName == Username)
        {
            UserStatus = "Offline";
        }
    }

    private void OnNewUserDetected(object? sender, User user)
    { 
        if (user.UserName == Username)
        {
            UserStatus = "Online";
        }
    }

    private async void OnUserStatusUpdated(object? sender, User user)
    {
        if (user.UserName == Username)
        {
            UserStatus = "Typing...";
            await Task.Delay(2000);
        }
    }

    public void UpdateUser(User user)
    {
        CurrentReceiverUser = user;
        Username = CurrentReceiverUser.UserName;
        UserStatus = CurrentReceiverUser.Status.ToString(); 
    }
}