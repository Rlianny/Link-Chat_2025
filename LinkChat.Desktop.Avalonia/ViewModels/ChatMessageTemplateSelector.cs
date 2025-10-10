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
                case TextMessage textMessage:
                    if (textMessage.UserName == GlobalSingletonHelper.MyUserName)
                        return SendedFileMessage?.Build(param); 
                    return ReceivedTextMessage?.Build(param);
                case File fileMessage:
                    if (fileMessage.UserName == GlobalSingletonHelper.MyUserName)
                        return SendedFileMessage?.Build(param);
                    return ReceivedFileMessage?.Build(param);
                default: return new TextBlock { Text = $"Not handled type: {message.GetType().Name}" };
            } 
        }
        return new TextBlock { Text = "Not valid object" };
    }

    public bool Match(object? data)
    {
        return data is ChatMessage;
    }
}