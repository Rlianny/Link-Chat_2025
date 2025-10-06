namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;
using System;

public class ChatWindowViewModel: ViewModelBase
{
    private ReceivedBubbleTextMessageViewModel _bubbleTextMessage;
    
    public ReceivedBubbleTextMessageViewModel BubbleTextMessage 
    { 
        get => _bubbleTextMessage;
        set => SetProperty(ref _bubbleTextMessage, value);
    }

    public ChatWindowViewModel()
    {
        TextMessage textMessage = new TextMessage("Lianny", DateTime.Now, "123", "Holaaaa");
        BubbleTextMessage = new ReceivedBubbleTextMessageViewModel(textMessage);
    }
}