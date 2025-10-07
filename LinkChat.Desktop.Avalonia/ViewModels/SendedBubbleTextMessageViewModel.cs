using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public class SendedBubbleTextMessageViewModel : BubbleMessageViewModel
{
    private TextMessage _textMessage;
    private string _content;
    private bool _confirmed; // pending to implement in core

    public string Content
    {
        get => _textMessage.Content;
    }

    public bool Confirmed
    {
        get => _confirmed;
    }
    
    public SendedBubbleTextMessageViewModel(TextMessage textMessage) : base(textMessage)
    {
        _textMessage = textMessage;
        _confirmed = false;
    }
}