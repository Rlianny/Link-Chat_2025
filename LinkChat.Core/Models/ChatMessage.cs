using System.Runtime.CompilerServices;
using LinkChat.Core.Interfaces;
namespace LinkChat.Core.Models;

public abstract class ChatMessage : Message, IReactable, IAckable
{
    public ChatMessage(string userName, DateTime timeStamp, string messageId) : base(userName, timeStamp)
    {
        this.messageId = messageId;
        reaction = Emoji.None;
    }
    public string MessageId => messageId;
    private string messageId;
    private Emoji reaction;
    public Emoji Reaction => reaction;
    private bool confirmed;
    public bool Confirmed => confirmed;

    public void SetReaction(Emoji reaction)
    {
        this.reaction = reaction;
    }

    public void Confirm()
    {
        confirmed = true;
    }
}
