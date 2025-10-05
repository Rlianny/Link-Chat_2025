using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkChat.Core.Models;
using LinkChat.Desktop.Avalonia.ViewModels;

namespace LinkChat.Desktop.Avalonia.Views;

public partial class ViewReceivedBubbleTextMessage : UserControl
{
    public static readonly StyledProperty<ReceivedBubbleTextMessageViewModel> BubbleTextMessageProperty =
        AvaloniaProperty.Register<ViewReceivedBubbleTextMessage, ReceivedBubbleTextMessageViewModel>(nameof(ReceivedBubbleTextMessageViewModel));
    
    public ReceivedBubbleTextMessageViewModel BubbleTextMessage
    {
        get => GetValue(BubbleTextMessageProperty);
        set => SetValue(BubbleTextMessageProperty, value);
    }
    public ViewReceivedBubbleTextMessage()
    {
        InitializeComponent();
    }
}