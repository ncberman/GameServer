using System.Reflection.PortableExecutable;

namespace GameServer.Source.Models.Database
{
    public class DatabaseUser
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int CharacterLimit { get; set; } = 1;
        public List<string> Characters { get; set; } = new();
    }
}
