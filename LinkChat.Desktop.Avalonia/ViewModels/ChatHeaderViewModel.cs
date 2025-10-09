using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public partial class ChatHeaderViewModel : ViewModelBase
{
    private string _username;
    public string Username
    {
        get => _username;
    }

    [ObservableProperty]
    private string _userStatus;

    public ChatHeaderViewModel(User user)
    {
        _username = user.UserName;
        _userStatus = user.Status.ToString();
    }
}