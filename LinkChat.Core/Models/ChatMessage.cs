using System.Runtime.CompilerServices;
using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public abstract class ChatMessage : Message, IReactable
{
    public ChatMessage(string userName, DateTime timeStamp, string messageId) : base(userName, timeStamp)
    {
        this.messageId = messageId;
    }
    public string MessageId => messageId;
    private string messageId;
    private Emoji reaction;
    public Emoji Reaction => reaction;

    public void SetReaction(Emoji reaction)
    {
        this.reaction = reaction;
    }
}
