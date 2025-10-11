using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

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
        _chatWindowViewModel = GlobalSingletonHelper.ChatWindowViewModel;
        DataContext = _chatWindowViewModel;
        
        _chatWindowViewModel.CurrentChatHistory.CollectionChanged += CurrentChatHistory_CollectionChanged;
    }
    
    private void CurrentChatHistory_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MessagesScrollViewer.ScrollToEnd();
        });
    }
    private async void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        await _chatWindowViewModel.TextBox_KeyDown(sender, e);
    }
}