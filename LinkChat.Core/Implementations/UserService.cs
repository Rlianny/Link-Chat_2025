using System.Collections.Concurrent;
using System.Threading.Tasks;
using LinkChat.Core.Models;
using LinkChat.Core.Services;
namespace LinkChat.Core.Implementations;

public class UserService : IUserService
{
    private User self;
    ConcurrentDictionary<string, User> Users = [];
    ConcurrentDictionary<string, DateTime> LastSeen = [];
    public event Action<User>? UserDisconnected;
    public event Action<User>? NewUserConnected;
    public event Action<User>? HeartbeatReceived;
    private IProtocolService protocolService;
    private INetworkService networkService;
    public UserService(IProtocolService protocolService, INetworkService networkService)
    {
        self = new User(" ", Gender.whatever, Status.Online, Tools.Tools.GetLocalMacAddress());
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
        bool isNew = false;
        if (!Users.ContainsKey(heartbeatMessage.UserName))
            isNew = true;

        LastSeen.AddOrUpdate(heartbeatMessage.UserName, addValue: DateTime.Now, updateValueFactory: (key, existing) => DateTime.Now);
        Users.AddOrUpdate(heartbeatMessage.UserName, addValue: new User(heartbeatMessage.UserName, heartbeatMessage.Gender, Status.Online, heartbeatMessage.MacAddress), (key, existing) => new User(heartbeatMessage.UserName, heartbeatMessage.Gender, Status.Online, heartbeatMessage.MacAddress));

        if (isNew)
            NewUserConnected?.Invoke(Users[heartbeatMessage.UserName]);
        else
            HeartbeatReceived?.Invoke(Users[heartbeatMessage.UserName]);
    }

    private void UpdateUsersStatuses()
    {
        Task task = Task.Run(async () =>
        {
            while (true)
            {
                SendHeartbeatRequest();
                PruneInactiveUsers();
                await Task.Delay(10000);
            }
        });
    }

    private void SendHeartbeatRequest()
    {
        var timestamp = DateTime.Now;
        HeartbeatMessage heartbeatToSend = new HeartbeatMessage(self.UserName, self.Gender, timestamp, self.MacAddress);
        byte[] frame = protocolService.CreateFrameToSend(null, heartbeatToSend, true);
        networkService.SendFrameAsync(frame, 0);
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
        throw new Exception($"User {userName} is not reachable by you MAC");
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
        throw new Exception($"User {userName} is not reachable by you NAME");
    }

    public Status GetUserStatusByName(string userName)
    {
        if (Users.ContainsKey(userName))
        {
            return Users[userName].Status;
        }
        throw new Exception($"User {userName} is not reachable by you STATUS");
    }

    public void PruneInactiveUsers()
    {
        foreach (var user in LastSeen)
        {
            int SecondsPassed = (int)(DateTime.Now - user.Value).TotalSeconds;
            if (SecondsPassed > 60)
            {
                if (Users.ContainsKey(user.Key))
                {
                    User disconnectedUser = Users[user.Key];
                    disconnectedUser.SetStatus(Status.Offline);
                    UserDisconnected?.Invoke(disconnectedUser);
                }
            }
        }
    }

    public void UpdateLastSeen(string userName)
    {
        LastSeen[userName] = DateTime.Now;
    }

    public void SetSelfUser(string name, Gender gender)
    {
        if (self.UserName != " ") return;
        self = new User(name, gender, Status.Online, Tools.Tools.GetLocalMacAddress());
        UpdateUsersStatuses();
    }
}