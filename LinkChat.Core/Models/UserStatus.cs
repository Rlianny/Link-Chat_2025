namespace LinkChat
{
    public class UserStatus : Message
    {
        Status userStatus;
        public UserStatus(string name, DateTime dateTime, Status userStatus) : base(name, dateTime)
        {
            this.userStatus = userStatus;
        }
    }
}