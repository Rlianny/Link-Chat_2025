namespace LinkChat.Core.Models;

public class User
{
    public string UserName
    {
        get { return userName; }
        private set { }
    }
    public byte[] MacAddress
    {
        get { return macAddress; }
        private set { }
    }
    public Status Status
    {
        get { return status; }
        private set { }
    }
    string userName;
    byte[] macAddress;
    Status status;
    public void SetStatus(Status Status)
    {
        status = Status;
    }
    public User(string name, Status status, byte[] macAddress)
    {
        (userName, this.status, this.macAddress) = (name, status, macAddress);
    }
}
