using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public abstract partial class BubbleMessageViewModel : ViewModelBase
{
    private ChatMessage _message;

    [ObservableProperty]
    private bool _isReactionMenuVisible;

    [ObservableProperty]
    private string _date;

    private Emoji _reaction; // possible backend synchronization problem with this field
    
    public BubbleMessageViewModel(ChatMessage chatMessage)
    {
        Console.WriteLine("BubbleMessageViewModel constructor called");
        _message = chatMessage;
        _reaction = _message.Reaction;
        _isReactionMenuVisible = false;
        _date = DateTime.Now.ToString("HH:mm");
        Console.WriteLine($"IsReactionMenuVisible initialized to: {_isReactionMenuVisible}");
    }

    [RelayCommand]
    private void ToggleReactionMenu()
    {
        Console.WriteLine("ToggleReactionMenu executed");
        IsReactionMenuVisible = !IsReactionMenuVisible;
        Console.WriteLine($"IsReactionMenuVisible is now: {IsReactionMenuVisible}");
    }
}