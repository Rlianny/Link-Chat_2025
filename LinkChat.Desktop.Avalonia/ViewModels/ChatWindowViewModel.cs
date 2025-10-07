using System.Reflection.Metadata;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;
using System;

public class ChatWindowViewModel: ViewModelBase
{
    private ReceivedBubbleTextMessageViewModel _message1;
    private ReceivedBubbleTextMessageViewModel _message2;
    private SendedBubbleTextMessageViewModel _message3;
    private ReceivedBubbleTextMessageViewModel _message4;
    private SendedBubbleTextMessageViewModel _message5;
    private ReceivedBubbleTextMessageViewModel _message6;
    private ReceivedBubbleFileMessageViewModel _receivedBubbleFileMessage;
    
    public ReceivedBubbleTextMessageViewModel Message1 
    { 
        get => _message1;
        set => SetProperty(ref _message1, value);
    }

    public ReceivedBubbleTextMessageViewModel Message2
    {
        get => _message2;
        set => SetProperty(ref _message2, value);
    }

    public SendedBubbleTextMessageViewModel Message3
    {
        get => _message3;
        set => SetProperty(ref  _message3, value);
    }
    
    public ReceivedBubbleTextMessageViewModel Message4
    {
        get => _message4;
        set => SetProperty(ref _message4, value);
    }
    
    public SendedBubbleTextMessageViewModel Message5
    {
        get => _message5;
        set => SetProperty(ref  _message5, value);
    }
    
    public ReceivedBubbleTextMessageViewModel Message6
    {
        get => _message6;
        set => SetProperty(ref _message6, value);
    }

    public ReceivedBubbleFileMessageViewModel FileMessage
    {
        get => _receivedBubbleFileMessage;
        set => SetProperty(ref _receivedBubbleFileMessage, value);
    }

    public ChatWindowViewModel()
    {
        TextMessage textMessage = new TextMessage("Bianca", DateTime.Now, "123", "Hola Diana!");
        _message1 = new ReceivedBubbleTextMessageViewModel(textMessage);
        
        TextMessage textMessage2 = new TextMessage("Bianca", DateTime.Now, "456", "Todo bien?");
        _message2 = new ReceivedBubbleTextMessageViewModel(textMessage2);
        
        TextMessage  textMessage3 = new TextMessage(" Diana", DateTime.Now, "123", "Hey, todo bien, y tú?");
        _message3 = new SendedBubbleTextMessageViewModel(textMessage3);
        
        TextMessage textMessage4 = new TextMessage("Bianca", DateTime.Now, "123", "Bien, te escribía para invitarte al cine esta tarde. Iremos con Alicia, te apuntas?");
        _message4 = new ReceivedBubbleTextMessageViewModel(textMessage4);
        
        TextMessage textMessage5 = new TextMessage("Diana", DateTime.Now, "123", "Por supuesto! Ahí estaré");
        _message5 = new SendedBubbleTextMessageViewModel(textMessage5);
        
        TextMessage textMessage6 = new TextMessage("Bianca", DateTime.Now, "123", "Perfecto, te envío la cartelera. Nos vemos!");
        _message6 = new ReceivedBubbleTextMessageViewModel(textMessage6);
        
        File file = new File("Bianca", DateTime.Now, "123", "path", 3.5, "Cartelera");
        _receivedBubbleFileMessage = new ReceivedBubbleFileMessageViewModel(file); 
    }
}