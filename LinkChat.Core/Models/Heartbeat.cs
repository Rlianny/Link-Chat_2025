namespace LinkChat
{
    public class HeartbeatMessage : Message
    {
        public string MacAddress { get { return macAddress; } private set { } }
        string macAddress;
        public HeartbeatMessage(string name, DateTime dateTime, string Mac) : base(name, dateTime)
        {
            macAddress = Mac;
        }
    }
}