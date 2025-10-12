namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // manage the list of active users and their statuses
    public interface IUserService
    {
        public List<User> GetAvailableUsers();
        public void UpdateLastSeen(string userName);
        public void PruneInactiveUsers();
        public User GetUserByName(string userName);
        public Status GetUserStatusByName(string userName);
        public byte[] GetMacAddress(string userName);
        public byte[] GetMacAddress(User user);
        public event Action<User>? UserDisconnected;
        public event Action<User>? NewUserConnected;
        public event Action<User>? HeartbeatReceived;
        public User GetSelfUser();
        public void SetSelfUser(string name, Gender gender);
    }
}