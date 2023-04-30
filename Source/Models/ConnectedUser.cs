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

        public ConnectedUser (string userId, string sessionId)
        {
            UserId = userId;
            SessionId = sessionId;
            var user = AddOrRetrieveUser().Result;
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
                var bytesRead = networkStream.Read(message, 0, 4096);
                if (bytesRead == 0) { Logger.Info($"User disconnected: {UserId}"); break; }

                IRequest request = SocketIO.ReadAndDeserialize<IRequest>(Encoding.ASCII.GetString(message, 0, bytesRead));
                if(request.SessionId != SessionId) { throw new BadSessionException("Unexpected session token"); }
                Logger.Info($"Request received {JsonConvert.SerializeObject(request)}");

                LastInput = DateTime.UtcNow;
                requestScheduler.EnqueueInput(request);
            }
        }

        private async Task<User> AddOrRetrieveUser()
        {
            var dbRef = new FirebaseRealtimeDatabase();
            var user = await dbRef.GetDataAsync<User>(String.Format(Constants.USER_DIR, UserId));
            if (user == null)
            {
                user = new()
                {
                    UserId = UserId,
                    Username = Username
                };
                await dbRef.AddDataAsync($"users/", $"user_{user.UserId}", user);
            }
            return user;
        }
    }
}
