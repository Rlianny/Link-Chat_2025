using Avalonia.Controls;
using Avalonia.Controls.Templates;
using LinkChat.Core.Models;

namespace LinkChat.Desktop.Avalonia.ViewModels;

public class ChatMessageTemplateSelector : IDataTemplate
{
    public IDataTemplate? ReceivedTextMessage { get; set; }
    public IDataTemplate? ReceivedFileMessage { get; set; }
    public IDataTemplate? SendedTextMessage { get; set; }
    public IDataTemplate? SendedFileMessage { get; set; }

    public Control Build(object? param)
    {
        if (param is ChatMessage message)
        {
            switch (message)
            {
                case
            } 
        }
        return new TextBlock { Text = "Objeto no válido" };
    }

    public bool Match(object? data)
    {
        return data is ChatMessage;
    }
}