namespace LinkChat.Core.Models;

public class HeartbeatMessage : Message
{
    public byte[] MacAddress { get { return macAddress; } private set { } }
    byte[] macAddress;
    public Gender Gender { get { return gender; } private set { } }
    private Gender gender;
    public HeartbeatMessage(string userName, Gender gender, DateTime timeStamp, byte[] macAddress) : base(userName, timeStamp)
    {
        this.macAddress = macAddress;
        this.gender = gender;
    }
}
