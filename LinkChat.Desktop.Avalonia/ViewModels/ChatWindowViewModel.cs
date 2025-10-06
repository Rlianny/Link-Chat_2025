namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;
using System;

public class ChatWindowViewModel: ViewModelBase
{
    public ReceivedBubbleTextMessageViewModel BubbleTextMessage { get; set; }
    public ChatWindowViewModel()
    {
        TextMessage textMessage = new TextMessage("Lianny", DateTime.Now, "123", "Holaaaa");
        ReceivedBubbleTextMessageViewModel bubble = new ReceivedBubbleTextMessageViewModel(textMessage);
        BubbleTextMessage = bubble;
    }
}