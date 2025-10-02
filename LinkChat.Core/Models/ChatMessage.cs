namespace LinkChat.Core.Models;
public abstract class ChatMessage : Message
{
    public ChatMessage(string userName, DateTime timeStamp) : base(userName, timeStamp)
    { }
}
