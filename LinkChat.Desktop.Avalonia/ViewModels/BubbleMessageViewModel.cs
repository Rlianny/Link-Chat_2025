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
    private AppManager _appManager;
    private ChatMessage _message;
    
    [ObservableProperty] private bool _isReactionMenuVisible = false;
    
    [ObservableProperty] private bool _isReactionVisible = false;
    
    [ObservableProperty] private string _date;

    [ObservableProperty] private Bitmap _emojiImage;
    
    [ObservableProperty] private Bitmap _confirmationSignal;
    
    private readonly Dictionary<string, string> reactionDictionary = new Dictionary<string, string>()
    {
        { "happy", "/Assets/Images/happy.png" },
        { "like", "/Assets/Images/like.png" },
        { "dislike", "/Assets/Images/dislike.png" },
        { "angry", "/Assets/Images/angry.png"},
        { "sad", "/Assets/Images/sad.png" },
        { "heart", "/Assets/Images/heart.png" },
    };
    public BubbleMessageViewModel(ChatMessage chatMessage, AppManager appManager)
    {
        _appManager = appManager;
        _message = chatMessage;
        _date = DateTime.Now.ToString("HH:mm");
        _emojiImage = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/happy.png"));
        _confirmationSignal = ImageHelper.LoadFromResource(new  Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/AcceptFontGrey.png"));
        
        if (_message.Reaction != Emoji.None)
        {
            ReactionSet();
        }

        appManager.ChatMessageConfirmed += OnChatMessageConfirmed;
        appManager.ReactedToMessage += OnReactedToMessage;
    }

    private void OnReactedToMessage(object? sender, ChatMessage message)
    {
        if (message.MessageId == _message.MessageId)
        {
            ReactionSet();    
        }
            
    }

    private void OnChatMessageConfirmed(object? sender, ChatMessage message)
    {
        if (message.MessageId == _message.MessageId)
        {
            ConfirmationSignal = ImageHelper.LoadFromResource(new  Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/Accept.png"));
        }
    }

    private void ReactionSet()
    {
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
        switch (emoji)
        {
           case "happy":
               _appManager.SendReaction(Emoji.HappyFace, _message.MessageId);
               break;
           case "like":
               _appManager.SendReaction(Emoji.Like, _message.MessageId);
               break;
           case "dislike":
               _appManager.SendReaction(Emoji.Dislike, _message.MessageId);
               break;
           case "angry":
               _appManager.SendReaction(Emoji.AngryFace, _message.MessageId);
               break;
           case "sad":
               _appManager.SendReaction(Emoji.SadFace, _message.MessageId);
               break;
           case  "heart":
               _appManager.SendReaction(Emoji.Heart, _message.MessageId);
               break;
        }
        IsReactionMenuVisible = !IsReactionMenuVisible;
    }
}