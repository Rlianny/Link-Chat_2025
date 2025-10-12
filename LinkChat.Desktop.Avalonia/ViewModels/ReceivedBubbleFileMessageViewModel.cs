using System;
using LinkChat.Core.Tools;

namespace LinkChat.Desktop.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using global::Avalonia.Media.Imaging;
using LinkChat.Core.Models;

public partial class ReceivedBubbleFileMessageViewModel : BubbleMessageViewModel
{
    private File _file;
    private string _fileName;
    private string _fileSize;

    public string FileName
    {
        get => _fileName;
        private set{}
    }

    public string FileSize
    {
        get => _fileSize;
        private set { }
    }
    
    [ObservableProperty]
    private Bitmap? _character;
    
    public ReceivedBubbleFileMessageViewModel(File file, AppManager appManager) : base(file,  appManager)
    {
        _file = file;
        _fileName = file.Name;
        _fileSize = Tools.FormatFileSize((long)file.Size);

        if (appManager.GetUserByName(_file.UserName).Gender == Gender.female)
            Character = GlobalSingletonHelper.FemaleCharacterOther;
        else
            Character = GlobalSingletonHelper.MaleCharacterOther;
    }
}