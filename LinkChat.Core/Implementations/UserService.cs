using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class UserService : IUserService
{
    private User self;
    Dictionary<string, User> Users = [];
    Dictionary<string, DateTime> LastSeen = [];
    public event Action<User>? UserDisconnected;
    public event Action<User>? NewUserConnected;
    private IProtocolService protocolService;
    private INetworkService networkService;
    public UserService(IProtocolService protocolService, INetworkService networkService, string selfUserName)
    {
        self = new User(selfUserName, Status.Online, Tools.Tools.GetLocalMacAddress());
        this.protocolService = protocolService;
        this.protocolService.HeartbeatFrameReceived += OnHeartbeatFrameReceived;
        this.networkService = networkService;
    }
    public User GetSelfUser()
    {
        return self;
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
            SendHeartbeatRequest();
            PruneInactiveUsers();
            await Task.Delay(10000);
        }
    }

    private void SendHeartbeatRequest()
    {
        HeartbeatMessage heartbeatToSend = new HeartbeatMessage(self.UserName, DateTime.Now, self.MacAddress);
        byte[] frame = protocolService.CreateFrameToSend(null, heartbeatToSend, true);
        networkService.SendFrameAsync(frame);
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