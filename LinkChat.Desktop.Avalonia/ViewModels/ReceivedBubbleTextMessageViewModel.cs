namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public class ReceivedBubbleTextMessageViewModel : BubbleMessageViewModel
{
    private TextMessage _textMessage; 
    public string Content
    {
        get { return _textMessage.Content.ToString(); }
        private set {}
    }
    
    public ReceivedBubbleTextMessageViewModel(TextMessage textMessage) : base(textMessage)
    {
        _textMessage = textMessage;
    }
}