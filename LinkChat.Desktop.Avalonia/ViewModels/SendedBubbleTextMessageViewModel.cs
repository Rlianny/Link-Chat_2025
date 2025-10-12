using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public partial class SendedBubbleTextMessageViewModel : BubbleMessageViewModel
{
    private TextMessage _textMessage;
    private string _content;

    public string Content
    {
        get => _textMessage.Content;
    }

    [ObservableProperty]
    private Bitmap? _character;
    
    public SendedBubbleTextMessageViewModel(TextMessage textMessage, AppManager appManager) : base(textMessage, appManager)
    {
        _textMessage = textMessage;
        if (appManager.GetCurrentSelfUser().Gender == Gender.female)
            Character = GlobalSingletonHelper.FemaleCharacterYou;
        else
            Character = GlobalSingletonHelper.MaleCharacterYou;
    }
}