using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public partial class ReceivedBubbleTextMessageViewModel : BubbleMessageViewModel
{
    private TextMessage _textMessage; 
    public string Content
    {
        get { return _textMessage.Content.ToString(); }
        private set {}
    }

    [ObservableProperty] private Image _userImage; 

    
    [ObservableProperty] 
    private bool _isReactionMenuVisible = false;
    partial void OnIsReactionMenuVisibleChanged(bool value)
    {
        // Notifica que el comando debe revisar si puede ejecutarse.
        (SelectReactionCommand as RelayCommand)?.NotifyCanExecuteChanged();
    }
    public ICommand ToggleReactionMenuCommand { get; }
    public ICommand SelectReactionCommand { get; }
    
    public ReceivedBubbleTextMessageViewModel(TextMessage textMessage) : base(textMessage)
    {
        _textMessage = textMessage;
        ToggleReactionMenuCommand = new RelayCommand(() => IsReactionMenuVisible = !IsReactionMenuVisible);
        SelectReactionCommand = new RelayCommand(ReactToMessage, CanReactToMessage);
    }
    
    private void ReactToMessage()
    {
        // 1. Lógica para asociar la reacción (emojiReaction) a este mensaje en el backend
        // ...

        // 2. (Opcional) Ocultar el menú después de reaccionar
        IsReactionMenuVisible = false;

        // 3. Puedes mostrar la reacción de otra manera, por ejemplo, con un pequeño icono al lado del mensaje
    }

    private bool CanReactToMessage()
    {
        return true;
    } 
}
