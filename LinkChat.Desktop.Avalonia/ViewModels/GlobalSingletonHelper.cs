using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public static class GlobalSingletonHelper
{
    public static readonly Bitmap? FemaleCharacterOther = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/FemaleCharacterOther.png"));
    public static readonly Bitmap? FemaleCharacterYou = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/FemaleCharacterYou.png"));
    public static readonly Bitmap? MaleCharacterOther = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/MaleCharacterOther.png"));
    public static readonly Bitmap? MaleCharacterYou = ImageHelper.LoadFromResource(new Uri("avares://LinkChat.Desktop.Avalonia/Assets/Images/MaleCharacterYou.png"));
    private static AppManager? _instance;
    public static AppManager AppManager 
    { 
        get 
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("AppManager must be initialized before use");
            }
            return _instance;
        }
        set => _instance = value; 
    }
    public static IStorageProvider? StorageProvider { get; set; }
    public static ChatWindowViewModel? ChatWindowViewModel { get; set; }
    public static User MyUser { get; private set; }
    public static void SetMyUser(User user)
    {
        MyUser = user;
    }
}