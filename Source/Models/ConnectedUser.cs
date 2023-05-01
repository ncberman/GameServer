using Common.Logging;
using GameLibrary;
using GameLibrary.Request;
using GameServer.Source.Exceptions;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Source.Models
{
    public class ConnectedUser
    {
        private static readonly ILog Logger = LogManager.GetLogger<ConnectedUser>();

        public string UserId { get; set; }
        public string Username { get; set; }
        public string SessionId { get; set; }
        public NetworkStream? NetworkStream { get; set; }

        public DateTime? LastInput { get; set; }

        public ConnectedUser (string userId, string username, string sessionId)
        {
            var user = AddOrRetrieveUser(userId, username).Result;
            UserId = user.UserId;
            Username = user.Username;

            SessionId = sessionId;
        }

        public void HandleConnection(NetworkStream networkStream, RequestScheduler requestScheduler)
        {
            NetworkStream = networkStream;
            var rateLimiter = new RateLimiter(threshold: AppSettings.GetValue<int>("Server:TickRate"), TimeSpan.FromSeconds(1));
            Logger.Info($"User connected: {UserId}");

            while (true)
            {
                if (!rateLimiter.CheckLimit()) break;

                byte[] message = new byte[4096];
                var bytesRead = NetworkStream.Read(message, 0, 4096);
                if (bytesRead == 0) { Logger.Info($"User disconnected: {UserId}"); break; }

                ServerRequest request = SocketIO.ReadAndDeserialize<ServerRequest>(Encoding.ASCII.GetString(message, 0, bytesRead));
                if(request.SessionId != SessionId) { throw new BadSessionException("Unexpected session token"); }
                Logger.Info($"Request received {JsonConvert.SerializeObject(request)}");

                LastInput = DateTime.UtcNow;
                requestScheduler.EnqueueInput(request);
            }
        }

        private async Task<User> AddOrRetrieveUser(string userId, string username)
        {
            try
            {
                var dbRef = new FirebaseRealtimeDatabase();
                var user = await dbRef.GetDataAsync<User>($"users/user_{UserId}");
                if (user == null)
                {
                    user = new()
                    {
                        UserId = userId,
                        Username = username
                    };
                    await dbRef.AddDataAsync($"users/", $"user_{user.UserId}", user);
                }
                return user;
            }
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
            }
            throw new Exception();
        }
    }
}
