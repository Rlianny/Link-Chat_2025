using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace LinkChat.Desktop.Avalonia.Views;
using LinkChat.Core.Models;
using LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Desktop.Avalonia.Views;

public partial class ChatWindow : Window
{
    ChatWindowViewModel _chatWindowViewModel;
    public ChatWindow()
    {
        InitializeComponent();
        _chatWindowViewModel = new ChatWindowViewModel();
        DataContext = _chatWindowViewModel;
        
        AppManager appManager = _chatWindowViewModel.AppManager;

       
        
        /*//File
        ReceivedBubbleFileMessageViewModel  bubbleFile = _chatWindowViewModel.FileMessage; 
        var bubbleComponentFile = this.FindControl<ViewReceivedBubbleFileMessage>("FileMessage");
        if (bubbleComponentFile != null)
        {
            bubbleComponentFile.ReceivedBubbleFileMessage =  bubbleFile;
        }*/
    }

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        _chatWindowViewModel.TextBox_KeyDown(sender, e);
    }
}