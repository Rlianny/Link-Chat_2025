using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace LinkChat.Desktop.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;

public abstract partial class BubbleMessageViewModel : ViewModelBase
{
    private ChatMessage _message;
    private DateTime _date;
    public string Date
    {
        get => _date.ToString("HH:mm");
        private set { }
    }
    private Emoji _reaction; // possible backend synchronization problem with this field
    
    [ObservableProperty] private bool _isReactionMenuVisible = false;
    public ICommand ToggleReactionMenuCommand { get; }
    public ICommand SelectReactionCommand { get; }
    public BubbleMessageViewModel(ChatMessage chatMessage)
    {
        _message = chatMessage;
        _reaction = _message.Reaction;
        ToggleReactionMenuCommand = new RelayCommand(() => IsReactionMenuVisible = !IsReactionMenuVisible);
        SelectReactionCommand = new RelayCommand<string>(ReactToMessage);
    }
    private void ReactToMessage(string emojiReaction)
    {
        // 1. Lógica para asociar la reacción (emojiReaction) a este mensaje en el backend
        // ...

        // 2. (Opcional) Ocultar el menú después de reaccionar
        IsReactionMenuVisible = false;

        // 3. Puedes mostrar la reacción de otra manera, por ejemplo, con un pequeño icono al lado del mensaje
    }
}