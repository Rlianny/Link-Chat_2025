namespace LinkChat.Core.Interfaces
{
    public interface IReactable
    {
        Emoji Reaction { get; }
        void SetReaction(Emoji emoji);
    }
}