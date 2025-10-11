using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public partial class ChatHeaderViewModel : ViewModelBase
{
    private AppManager _appManager;
    
    private CancellationTokenSource? _typingCancellationTokenSource;
    
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
            // Cancel any existing typing timer
            _typingCancellationTokenSource?.Cancel();
            _typingCancellationTokenSource = new CancellationTokenSource();
            var token = _typingCancellationTokenSource.Token;

            // Store the current status
            var originalStatus = CurrentReceiverUser?.Status.ToString() ?? "Online";

            // Show typing status
            UserStatus = "Typing...";

            try
            {
                // Wait for 2 seconds then revert to original status
                await Task.Delay(TimeSpan.FromSeconds(2), token);

                // Only update if we haven't been canceled by a new typing notification
                if (!token.IsCancellationRequested)
                {
                    UserStatus = originalStatus;
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, ignore
            }
            finally
            {
                if (_typingCancellationTokenSource?.Token == token)
                {
                    _typingCancellationTokenSource?.Dispose();
                    _typingCancellationTokenSource = null;
                }
            }
        }
    }

    public void UpdateUser(User user)
    {
        CurrentReceiverUser = user;
        Username = CurrentReceiverUser.UserName;
        UserStatus = CurrentReceiverUser.Status.ToString(); 
    }
}