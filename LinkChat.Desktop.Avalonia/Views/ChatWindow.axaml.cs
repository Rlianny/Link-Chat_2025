using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LinkChat.Desktop.Avalonia.Views;
using LinkChat.Core.Models;
using LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Desktop.Avalonia.Views;

public partial class ChatWindow : Window
{
    public ReceivedBubbleTextMessageViewModel BubbleTextMessage { get; set; }
    public ChatWindow()
    {
        InitializeComponent();
        DataContext = new ChatWindowViewModel();
        TextMessage textMessage = new TextMessage("Lianny", DateTime.Now, "123", "Holaaaa");
        ReceivedBubbleTextMessageViewModel bubble = new ReceivedBubbleTextMessageViewModel(textMessage);
        BubbleTextMessage = bubble;
        
    }
}