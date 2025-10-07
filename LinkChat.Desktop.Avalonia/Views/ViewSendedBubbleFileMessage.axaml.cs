using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkChat.Desktop.Avalonia.ViewModels;

namespace LinkChat.Desktop.Avalonia.Views;

public partial class ViewSendedBubbleFileMessage : UserControl
{
    public static readonly StyledProperty<SendedBubbleFileMessageViewModel> SendedFileMessageProperty =
        AvaloniaProperty.Register<ViewSendedBubbleFileMessage, SendedBubbleFileMessageViewModel>(nameof(SendedBubbleFileMessageViewModel));

    public SendedBubbleFileMessageViewModel SendedFileMessage
    {
        get => GetValue(SendedFileMessageProperty);
        set => SetValue(SendedFileMessageProperty, value);
    }
    public ViewSendedBubbleFileMessage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == SendedFileMessageProperty)
            {
                DataContext = SendedFileMessage;
            }
        };
    }
}