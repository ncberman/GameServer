using GameLibrary.Model;

namespace GameServer.Source.Models
{
    public class Instance
    {
        public Guid InstanceId = Guid.NewGuid();
        public string Name { get; private set; }
        private ushort PlayerCap = 100;
        public bool Filled = false;

        public Dictionary<string, SharedCharacter> ClientCharacters { get; private set; }

        public Instance(string name)
        {
            ClientCharacters = new();
            Name = name;
        }

        public void AddCharacter(string clientId, SharedCharacter character)
        {
            if (ClientCharacters.Count >= PlayerCap) return;
            ClientCharacters.Add(clientId, character);
        }
    }
}
