namespace LinkChat
{
    public class UserStatus : Message
    {
        public Status UsrStatus { get { return userStatus; } private set { } }
        Status userStatus;
        public UserStatus(string name, DateTime dateTime, Status userStatus) : base(name, dateTime)
        {
            this.userStatus = userStatus;
        }
    }
}