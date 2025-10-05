namespace LinkChat.Desktop.Avalonia.ViewModels;
using LinkChat.Core.Models;

public class ReceivedBubbleFileMessageViewModel : BubbleMessageViewModel
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
        private set{}
    }
    
    public ReceivedBubbleFileMessageViewModel(File file) : base(file)
    {
        _file = file;
        _fileName = file.Name;
        _fileSize = file.Size.ToString();
    }
}