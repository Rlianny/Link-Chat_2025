using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public partial class ReceivedBubbleTextMessageViewModel : BubbleMessageViewModel
{
    private TextMessage _textMessage; 
    public string Content
    {
        get { return _textMessage.Content;}
        private set {}
    }
    
    public ReceivedBubbleTextMessageViewModel(TextMessage textMessage) : base(textMessage)
    {
        _textMessage = textMessage;
    }
}
