namespace LinkChat.Core.Models;

public class UserStatus : Message
{
    public Status UsrStatus { get { return userStatus; } private set { } }
    Status userStatus;
    public UserStatus(string userName, DateTime timeStamp, Status userStatus) : base(userName, timeStamp)
    {
        this.userStatus = userStatus;
    }
}
