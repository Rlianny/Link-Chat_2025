using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace LinkChat.Desktop.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;

public abstract partial class BubbleMessageViewModel : ViewModelBase
{
    public Bitmap? UserImage { get; } = new Bitmap(AssetLoader.Open(new Uri("Assets/Images/Character1.png")));
    private ChatMessage _message;
    private DateTime _date;

    [ObservableProperty]
    private bool isReactionMenuVisible = false;
    public string Date
    {
        get => _date.ToString("HH:mm");
        private set { }
    }
    private Emoji _reaction; // possible backend synchronization problem with this field
    
    public BubbleMessageViewModel(ChatMessage chatMessage)
    {
        _message = chatMessage;
        _reaction = _message.Reaction;
    }
    public void ToggleReactionMenu()
    {
        isReactionMenuVisible = !isReactionMenuVisible;
    }

    public bool CanToggleReactionMenu()
    {
        return true;
    }
}