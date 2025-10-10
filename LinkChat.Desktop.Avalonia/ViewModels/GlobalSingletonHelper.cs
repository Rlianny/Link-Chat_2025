using Avalonia.Platform.Storage;

namespace LinkChat.Desktop.Avalonia.ViewModels;

internal static class GlobalSingletonHelper
{
    public static string MyUserName {get; private set;}
    public static IStorageProvider StorageProvider { get; set; }

    public static void SetUserName(string myUserName)
    {
        MyUserName = myUserName;
    }
}