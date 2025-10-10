using Avalonia.Platform.Storage;

namespace LinkChat.Desktop.Avalonia.ViewModels;

internal static class GlobalSingletonHelper
{
    public static IStorageProvider StorageProvider { get; set; }
}