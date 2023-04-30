using System.Reflection.PortableExecutable;

namespace GameServer.Source.Models.Database
{
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public Character[] Characters { get; set; } = new Character[1];
    }
}
