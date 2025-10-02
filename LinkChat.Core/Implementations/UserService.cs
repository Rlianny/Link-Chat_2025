using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class UserService : IUserService
{
    User Self;
    Dictionary<string, User> Users = [];
    Dictionary<string, DateTime> LastSeen = [];
    public event Action<User>? UserDisconnected;
    public event Action<User>? NewUserConnected;
    public event Action<User>? HeartbeatRequest;
    public UserService(IProtocolService protocolService, string selfUserName, byte[] selfMacAddress)
    {
        Self = new User(selfUserName, Status.Online, selfMacAddress);
        protocolService.HeartbeatFrameReceived += OnHeartbeatFrameReceived;
    }
    public User GetSelfUser()
    {
        return Self;
    }
    private void OnHeartbeatFrameReceived(HeartbeatMessage heartbeatMessage)
    {
        if (LastSeen.ContainsKey(heartbeatMessage.UserName))
        {
            LastSeen[heartbeatMessage.UserName] = DateTime.Now;
        }
        else
        {
            LastSeen.Add(heartbeatMessage.UserName, DateTime.Now);
        }
        if (Users.ContainsKey(heartbeatMessage.UserName))
        {
            Users[heartbeatMessage.UserName].SetStatus(Status.Online);
        }
        else
        {
            Users.Add(heartbeatMessage.UserName, new User(heartbeatMessage.UserName, Status.Online, heartbeatMessage.MacAddress));
        }
    }
    public void UpdateUserStatuses()
    {
        Task task = Task.Run(UpdateUsersStatuses);
    }
    private async void UpdateUsersStatuses()
    {
        while (true)
        {
            HeartbeatRequest.Invoke(Self);
            PruneInactiveUsers();
            await Task.Delay(10000);
        }
    }

    public List<User> GetAvailableUsers()
    {
        List<User> AvailableUsers = [];
        foreach (var user in Users.Values)
        {
            if (user.Status == Status.Online)
            {
                AvailableUsers.Add(user);
            }
        }
        return AvailableUsers;
    }

    public byte[] GetMacAddress(string userName)
    {
        if (Users.ContainsKey(userName))
        {
            return Users[userName].MacAddress;
        }
        throw new Exception($"User {userName} is not reachable by you");
    }

    public byte[] GetMacAddress(User user)
    {
        return user.MacAddress;
    }

    public User GetUserByName(string userName)
    {
        if (Users.ContainsKey(userName))
        {
            return Users[userName];
        }
        throw new Exception($"User {userName} is not reachable by you");
    }

    public Status GetUserStatusByName(string userName)
    {
        if (Users.ContainsKey(userName))
        {
            return Users[userName].Status;
        }
        throw new Exception($"User {userName} is not reachable by you");
    }

    public void PruneInactiveUsers()
    {
        foreach (var user in LastSeen)
        {
            int SecondsPassed = (int)(DateTime.Now - user.Value).TotalSeconds;
            if (SecondsPassed > 30)
            {
                if (Users.ContainsKey(user.Key))
                {
                    Users.Remove(user.Key);
                }
            }
        }
    }
}