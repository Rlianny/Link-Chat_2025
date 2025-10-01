namespace LinkChat.Core.Models
{
    public class User
    {
        string userName;
        Status status;

        public User(string name, Status status)
        {
            (userName, this.status) = (name, status);

        }
    }
}