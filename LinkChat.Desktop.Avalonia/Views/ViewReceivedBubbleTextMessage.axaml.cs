using System;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Set DataContext immediately if BubbleTextMessage is already set
        if (BubbleTextMessage != null)
        {
            DataContext = BubbleTextMessage;
        }

        // Listen for property changes
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == BubbleTextMessageProperty)
            {
                DataContext = BubbleTextMessage;
            }
        };
    }
}