using GameLibrary.Request;
using GameServer.Source.Models;
using GameServer.Source.Services;
using System.Collections.Concurrent;

namespace GameServer.Source.Components
{
    public class PlayerManager
    {
        private static readonly Lazy<PlayerManager> lazyInstance = new(() => new PlayerManager());
        public static PlayerManager Instance => lazyInstance.Value;

        ConcurrentDictionary<string, ClientController> ConnectedPlayers = new();

        public PlayerManager() 
        {

        }

        public async Task AddNewConnection(string uid, ClientController newClient)
        {
            if(ConnectedPlayers.TryAdd(uid, newClient))
            {
                await newClient.HandleConnection();
            } 
            else 
            {
                // This account is already connected
            }
        }
    }
}
