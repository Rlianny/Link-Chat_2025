namespace LinkChat.Core.Interfaces
{
    public interface IReactable
    {
        public Emoji Reaction { get; }
        public void SetReaction(Emoji emoji);
    }
}