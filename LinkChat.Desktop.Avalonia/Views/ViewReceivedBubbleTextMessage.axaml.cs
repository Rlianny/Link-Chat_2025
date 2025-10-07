using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkChat.Core.Models;
using LinkChat.Desktop.Avalonia.ViewModels;

namespace LinkChat.Desktop.Avalonia.Views;

public partial class ViewReceivedBubbleTextMessage : UserControl
{
    public static readonly StyledProperty<ReceivedBubbleTextMessageViewModel> ReceivedBubbleTextMessageProperty =
        AvaloniaProperty.Register<ViewReceivedBubbleTextMessage, ReceivedBubbleTextMessageViewModel>(nameof(ReceivedBubbleTextMessageViewModel));
    
    public ReceivedBubbleTextMessageViewModel ReceivedBubbleTextMessage
    {
        get => GetValue(ReceivedBubbleTextMessageProperty);
        set => SetValue(ReceivedBubbleTextMessageProperty, value);
    }
    public ViewReceivedBubbleTextMessage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Set DataContext immediately if BubbleTextMessage is already set
        if (ReceivedBubbleTextMessage is not null)
        {
            DataContext = ReceivedBubbleTextMessage;
        }

        // Listen for property changes
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == ReceivedBubbleTextMessageProperty)
            {
                DataContext = ReceivedBubbleTextMessage;
            }
        };
    }
}