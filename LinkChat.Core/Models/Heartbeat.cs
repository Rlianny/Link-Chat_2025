namespace LinkChat.Core.Models;

public class HeartbeatMessage : Message
{
    public string MacAddress { get { return macAddress; } private set { } }
    string macAddress;
    public HeartbeatMessage(string userName, DateTime timeStamp, string macAddress) : base(userName, timeStamp)
    {
        this.macAddress = macAddress;
    }
}
