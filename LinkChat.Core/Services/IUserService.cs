namespace LinkChat.Core
{
    // manage the list of active users and their statuses
    public interface IUserService
    {
        public List<User> GetAvailableUsers();
        public User GetUserByName();
        public Status GetUserStatusByName();
        public string GetMacAddress(string userName);
        public string GetMacAddress(User user);
    }
}