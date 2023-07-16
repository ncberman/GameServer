namespace GameServer.Source.Models.Database
{
    public class DatabaseUsers
    {
        public DatabaseUsers() { UserList = new DatabaseUser[] { new DatabaseUser() }; }
        public DatabaseUser[] UserList { get; set; }
    }
}
