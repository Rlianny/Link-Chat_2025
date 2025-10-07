using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkChat.Desktop.Avalonia.ViewModels;

namespace LinkChat.Desktop.Avalonia.Views;

public partial class ViewReceivedBubbleFileMessage : UserControl
{
    

    public static readonly StyledProperty<ReceivedBubbleFileMessageViewModel> ReceivedBubbleFileMessageProperty =
        AvaloniaProperty.Register<ViewReceivedBubbleFileMessage, ReceivedBubbleFileMessageViewModel>(nameof(ReceivedBubbleFileMessageViewModel));

    public ReceivedBubbleFileMessageViewModel ReceivedBubbleFileMessage
    {
        get => GetValue(ReceivedBubbleFileMessageProperty);
        set => SetValue(ReceivedBubbleFileMessageProperty, value);
    }
    public ViewReceivedBubbleFileMessage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        if (ReceivedBubbleFileMessage is not null)
        {
            DataContext = ReceivedBubbleFileMessage;
        }

        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == ReceivedBubbleFileMessageProperty)
            {
                DataContext = ReceivedBubbleFileMessage;
            }

        };
    }
}