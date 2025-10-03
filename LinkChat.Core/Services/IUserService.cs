namespace LinkChat.Core.Services
{
    using LinkChat.Core.Models;

    // manage the list of active users and their statuses
    public interface IUserService
    {
        public List<User> GetAvailableUsers();
        public void UpdateUsersStatuses();
        public void PruneInactiveUsers();
        public User GetUserByName(string userName);
        public Status GetUserStatusByName(string userName);
        public byte[] GetMacAddress(string userName);
        public byte[] GetMacAddress(User user);
        public event Action<User>? UserDisconnected;
        public event Action<User>? NewUserConnected;
        public User GetSelfUser();
    }
}