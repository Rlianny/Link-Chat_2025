using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkChat.Desktop.Avalonia.ViewModels;

namespace LinkChat.Desktop.Avalonia.Views;

public partial class ViewSendedBubbleTextMessage : UserControl
{
    public static readonly StyledProperty<SendedBubbleTextMessageViewModel> SendedBubbleTextMessageProperty =
        AvaloniaProperty.Register<ViewSendedBubbleTextMessage, SendedBubbleTextMessageViewModel>(
            nameof(SendedBubbleTextMessageViewModel));

    public SendedBubbleTextMessageViewModel SendedBubbleTextMessage
    {
        get => GetValue(SendedBubbleTextMessageProperty);
        set => SetValue(SendedBubbleTextMessageProperty, value);
    }
    public ViewSendedBubbleTextMessage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == SendedBubbleTextMessageProperty)
            {
                DataContext = SendedBubbleTextMessage;
            }
        };
    }
}