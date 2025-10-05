namespace LinkChat.Desktop.Avalonia.ViewModels;

public class ErrorMessageViewModel : ViewModelBase
{
    private string _errorMessage;
    public string ErrorMessage
    {
        get { return _errorMessage; }
    }

    public ErrorMessageViewModel(string errorMessage)
    {
        _errorMessage = errorMessage;
    }
}