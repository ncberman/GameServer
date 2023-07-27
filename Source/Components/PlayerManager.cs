using Common.Logging;
using GameServer.Source.Models;
using System.Collections.Concurrent;

namespace GameServer.Source.Components
{
    public class PlayerManager
    {
        private static readonly ILog Logger = LogManager.GetLogger<PlayerManager>();
        private static readonly Lazy<PlayerManager> lazyInstance = new(() => new PlayerManager());
        public static PlayerManager Instance => lazyInstance.Value;

        public readonly ConcurrentDictionary<string, ClientController> ConnectedPlayers = new();

        public PlayerManager() 
        {

        }

        public async Task AddNewConnection(string uid, ClientController newClient)
        {
            if(ConnectedPlayers.TryAdd(uid, newClient))
            {
                await newClient.HandleConnection();
                ConnectedPlayers.Remove(uid, out _);
            } 
            else 
            {
                Logger.Info($"Player {uid} is already connected!");
                // This account is already connected
            }
        }

        public ClientController GetClientById(string id)
        {
            ConnectedPlayers.TryGetValue(id, out var client);
            return client;
        }
    }
}
