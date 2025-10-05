using System;
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
    
    public BubbleMessageViewModel(ChatMessage chatMessage)
    {
        _message = chatMessage;
        _reaction = _message.Reaction;
    }

    [ObservableProperty] private bool _isReactionMenuVisible;

    [RelayCommand]
    private void ToggleReactionMenu()
    {
        IsReactionMenuVisible = !IsReactionMenuVisible;
    }

    [RelayCommand]
    private void ReactToMessage(String emoji)
    {
        // Aquí va tu lógica para manejar la reacción.
        // Por ejemplo, enviar la reacción al servidor.
        Console.WriteLine($"Reacción seleccionada: {emoji}");

        // Opcional: Ocultar el menú después de reaccionar.
        IsReactionMenuVisible = false;
    }

}