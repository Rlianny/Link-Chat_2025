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
        //Console.WriteLine($"Un hearbeat ha llegado de {heartbeatMessage.UserName}");
        LastSeen.AddOrUpdate(heartbeatMessage.UserName, addValue: DateTime.Now, updateValueFactory: (key, existing) => DateTime.Now);
        Users.AddOrUpdate(heartbeatMessage.UserName, addValue: new User(heartbeatMessage.UserName, Status.Online, heartbeatMessage.MacAddress), (key, existing) => new User(heartbeatMessage.UserName, Status.Online, heartbeatMessage.MacAddress));

    }
    public async Task UpdateUserStatuses()
    {
        Task task = Task.Run(UpdateUsersStatuses);
        await task;
    }
    private async void UpdateUsersStatuses()
    {
        while (true)
        {
            SendHeartbeatRequest();
            PruneInactiveUsers();
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
                    Users.TryRemove(user.Key, out User disconnectedUser);
                }
            }
        }
    }

    void IUserService.UpdateUsersStatuses()
    {
        UpdateUsersStatuses();
    }

    public void UpdateLastSeen(string userName)
    {
        LastSeen[userName] = DateTime.Now;
    }
}