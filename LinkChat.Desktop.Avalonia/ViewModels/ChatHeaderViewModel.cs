namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public class ChatHeaderViewModel : ViewModelBase
{
    private string _username;
    public string Username
    {
        get => _username;
    }

    private string _userStatus;
    public string UserStatus
    {
        get => _userStatus;
    }

    public ChatHeaderViewModel(User user)
    {
        _username = user.UserName;
        _userStatus = user.Status.ToString();
    }
}