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
        if (Username == " ")
            UserStatus = " ";

        _appManager = appManager;
        _appManager.UserStatusUpdated += OnUserStatusUpdated;
        _appManager.NewUserDetected += OnNewUserDetected;
        _appManager.UserPruned += OnUserPruned;
        _appManager.HeartbeatReceived += OnHeartbeatReceived;
    }

    private void OnHeartbeatReceived(object? sender, User e)
    {
        if (e.UserName == Username)
        {
            UserStatus = "Online";
        }
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

            // Save the original status before changing to "Typing..."
            // Don't use current status as it might already be "Typing..."
            string originalStatus = CurrentReceiverUser?.Status == Status.Online ? "Online" : "Offline";
        
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