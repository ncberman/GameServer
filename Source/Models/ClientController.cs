using Common.Logging;
using GameLibrary.Request;
using GameLibrary.Request.Util;
using GameServer.Source.Exceptions;
using GameServer.Source.Handlers;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Source.Models
{
    public class ClientController
    {
        private static readonly ILog Logger = LogManager.GetLogger<ClientController>();

        public string UserId { get; set; }
        public string Username { get; set; }
        public string SessionId { get; set; }
        public NetworkStream? NetworkStream { get; set; }

        public DateTime? LastInput { get; set; }

        public ClientController (string userId, string username, string sessionId)
        {
            var user = FirebaseService.AddOrRetrieveUser(userId, username).Result;
            UserId = user.UserId;
            Username = user.Username;

            SessionId = sessionId;
        }

        public void HandleConnection(NetworkStream networkStream, TickBasedScheduler requestScheduler)
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
                if (request.Request is IRealtimeRequest) 
                {
                    IRealtimeHandler handler = RealtimeHandlerFactory.GetHandler(request);
                    handler.HandleRequest();
                }
                else if(request.Request is ITickBasedRequest)
                {
                    requestScheduler.EnqueueInput(request);
                }
                else
                {
                    throw new UnsupportedRequestTypeException("Request was not Realtime or TickBased");
                }
            }
        }
    }
}
