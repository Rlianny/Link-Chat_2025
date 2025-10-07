using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public abstract partial class BubbleMessageViewModel : ViewModelBase
{
    private ChatMessage _message;
    
    [ObservableProperty] private bool _isReactionMenuVisible = false;
    
    [ObservableProperty] private bool _isReactionVisible = false;
    
    [ObservableProperty] private string _date;

    [ObservableProperty] private Bitmap _emojiImage; 
    
    private readonly Dictionary<string, string> reactionDictionary = new Dictionary<string, string>()
    {
        { "happy", "/Assets/Images/happy.png" },
        { "like", "/Assets/Images/like.png" },
        { "dislike", "/Assets/Images/dislike.png" },
        { "angry", "/Assets/Images/angry.png"},
        { "sad", "/Assets/Images/sad.png" },
        { "heart", "/Assets/Images/heart.png" },
    };
    public BubbleMessageViewModel(ChatMessage chatMessage)
    {
        _message = chatMessage;
        _date = DateTime.Now.ToString("HH:mm");
        _emojiImage = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/happy.png"));
        
        if (_message.Reaction != Emoji.None)
        {
            ReactionSet();
        }
    }
    
    private void ReactionSet()
    {
        Console.WriteLine("Se notificó correctamente");
        string emoji = "";
        switch (_message.Reaction)
        {
            case Emoji.Like: emoji = "like"; break;
            case Emoji.AngryFace: emoji = "angry"; break;
            case Emoji.Dislike:  emoji = "dislike"; break;
            case Emoji.HappyFace:  emoji = "happy"; break;
            case Emoji.Heart:   emoji = "heart"; break;
            case Emoji.SadFace:  emoji = "sad"; break;
        }
        
        EmojiImage =ImageHelper.LoadFromResource(new Uri($"avares://LinkChat.Desktop.Avalonia{reactionDictionary[emoji]}"));
        IsReactionVisible = true;
    }

    [RelayCommand]
    private void ToggleReactionMenu()
    {
        IsReactionMenuVisible = !IsReactionMenuVisible;
    }

    [RelayCommand]
    private void React(string emoji)
    {
        Console.WriteLine($"You reacted with {emoji} to a message");
        switch (emoji)
        {
           case "happy":
               _message.SetReaction(Emoji.HappyFace);
               break;
           case "like":
               _message.SetReaction(Emoji.Like);
               break;
           case "dislike":
               _message.SetReaction(Emoji.Dislike);
               break;
           case "angry":
               _message.SetReaction(Emoji.AngryFace);
               break;
           case "sad":
               _message.SetReaction(Emoji.SadFace);
               break;
           case  "heart":
               _message.SetReaction(Emoji.Heart);
               break;
        }
        ;
        ReactionSet();
        IsReactionMenuVisible = !IsReactionMenuVisible;
    }
    
}