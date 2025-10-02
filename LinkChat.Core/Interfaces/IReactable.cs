namespace LinkChat
{
    public interface IReactable
    {
        Emoji Reaction { get; }
        void SetReaction(Emoji emoji);
    }
}