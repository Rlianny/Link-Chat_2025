using System;
using Avalonia.Platform.Storage;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public static class GlobalSingletonHelper
{
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

    public static string? MyUserName {get; private set;}
    public static IStorageProvider? StorageProvider { get; set; }
    public static ChatWindowViewModel? ChatWindowViewModel { get; set; }

    public static void SetUserName(string myUserName)
    {
        MyUserName = myUserName;
    }
}