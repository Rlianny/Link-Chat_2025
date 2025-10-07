namespace LinkChat.Core.Interfaces
{
    public interface IAckable
    {
        public bool Confirmed { get; }
        public void Confirm();
    }
}