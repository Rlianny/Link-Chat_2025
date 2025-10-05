using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace LinkChat.Desktop.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;

public abstract partial class BubbleMessageViewModel : ViewModelBase
{
    private ChatMessage _message;
    private DateTime _date;
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
}