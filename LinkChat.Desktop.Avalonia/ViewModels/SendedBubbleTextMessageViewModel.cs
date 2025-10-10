using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public class SendedBubbleTextMessageViewModel : BubbleMessageViewModel
{
    private TextMessage _textMessage;
    private string _content;

    public string Content
    {
        get => _textMessage.Content;
    }
    
    public SendedBubbleTextMessageViewModel(TextMessage textMessage, AppManager appManager) : base(textMessage, appManager)
    {
        _textMessage = textMessage;
    }
}