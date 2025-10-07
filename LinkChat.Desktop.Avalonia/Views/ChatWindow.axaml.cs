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
    public ChatWindow()
    {
        InitializeComponent();
        DataContext = new ChatWindowViewModel();

        // Message 1 
        TextMessage textMessage1 = new TextMessage(" Bianca", DateTime.Now, "123", "Hola Diana!");
        ReceivedBubbleTextMessageViewModel bubble1 = new ReceivedBubbleTextMessageViewModel(textMessage1);
        var bubbleComponent = this.FindControl<ViewReceivedBubbleTextMessage>("Message1");
        if (bubbleComponent != null)
        {
            bubbleComponent.ReceivedBubbleTextMessage = bubble1;
        }
        
        // Message 2
        TextMessage textMessage2 = new TextMessage(" Bianca", DateTime.Now, "123", "Todo bien?");
        ReceivedBubbleTextMessageViewModel bubble2 = new ReceivedBubbleTextMessageViewModel(textMessage2);
        var bubbleComponent2 = this.FindControl<ViewReceivedBubbleTextMessage>("Message2");
        if (bubbleComponent2 != null)
        {
            bubbleComponent2.ReceivedBubbleTextMessage = bubble2;
        }
        
        // Message 3
        TextMessage  textMessage3 = new TextMessage(" Diana", DateTime.Now, "123", "Hey, todo bien, y tú?");
        SendedBubbleTextMessageViewModel bubble3 = new SendedBubbleTextMessageViewModel(textMessage3);
        var bubbleComponent3 = this.FindControl<ViewSendedBubbleTextMessage>("Message3");
        if (bubbleComponent3 != null)
        {
            bubbleComponent3.SendedBubbleTextMessage = bubble3;
        }
        
        // Message 4
        TextMessage textMessage4 = new TextMessage("Bianca", DateTime.Now, "123", "Bien, te escribía para invitarte al cine esta tarde. Iremos con Alicia, te apuntas?");
        ReceivedBubbleTextMessageViewModel bubble4 = new ReceivedBubbleTextMessageViewModel(textMessage4);
        var bubbleComponent4 = this.FindControl<ViewReceivedBubbleTextMessage>("Message4");
        if (bubbleComponent4 != null)
        {
            bubbleComponent4.ReceivedBubbleTextMessage = bubble4;
        }
        
        //Message 5
        TextMessage textMessage5 = new TextMessage("Diana", DateTime.Now, "123", "Por supuesto! Ahí estaré");
        SendedBubbleTextMessageViewModel bubble5 = new SendedBubbleTextMessageViewModel(textMessage5);
        var bubbleComponent5 = this.FindControl<ViewSendedBubbleTextMessage>("Message5");
        if (bubbleComponent5 != null)
        {
            bubbleComponent5.SendedBubbleTextMessage = bubble5;
        }
        
        //Message 6
        TextMessage textMessage6 = new TextMessage("Bianca", DateTime.Now, "123", "Perfecto, te envío la cartelera. Nos vemos!");
        ReceivedBubbleTextMessageViewModel bubble6 = new ReceivedBubbleTextMessageViewModel(textMessage6);
        var bubbleComponent6 = this.FindControl<ViewReceivedBubbleTextMessage>("Message6");
        if (bubbleComponent6 != null)
        {
            bubbleComponent6.ReceivedBubbleTextMessage = bubble6;
        }
        
        //File
        File file = new File("Bianca", DateTime.Now, "123", "path", 3.5, "Cartelera");
        ReceivedBubbleFileMessageViewModel  bubbleFile = new ReceivedBubbleFileMessageViewModel(file);
        var bubbleComponentFile = this.FindControl<ViewReceivedBubbleFileMessage>("FileMessage");
        if (bubbleComponentFile != null)
        {
            bubbleComponentFile.ReceivedBubbleFileMessage =  bubbleFile;
        }
    }
}