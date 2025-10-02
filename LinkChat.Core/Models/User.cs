namespace LinkChat.Core.Models
{
    public class User
    {
        public string UserName { get { return userName; } private set { } }
        public Status Status { get { return status; } private set { } }
        string userName;
        Status status;

        public User(string name, Status status)
        {
            (userName, this.status) = (name, status);

        }
    }
}