public class HeartbeatMessage : Message
{
    string macAddress;
    public HeartbeatMessage(string name, DateTime dateTime, string Mac) : base(name, dateTime)
    {
        macAddress = Mac;
    }
}