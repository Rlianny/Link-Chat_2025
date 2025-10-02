namespace LinkChat.Core.Models;

public class HeartbeatMessage : Message
{
    public byte[] MacAddress { get { return macAddress; } private set { } }
    byte[] macAddress;
    public HeartbeatMessage(string userName, DateTime timeStamp, byte[] macAddress) : base(userName, timeStamp)
    {
        this.macAddress = macAddress;
    }
}
