namespace GameServer.Source.Models.Database
{
    public class Users
    {
        public Users() { UserList = new User[] { new User() }; }
        public User[] UserList { get; set; }
    }
}
