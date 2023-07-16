using Common.Logging;
using GameLibrary.Request;
using GameLibrary.Request.Util;
using GameServer.Source.Components;
using GameServer.Source.Exceptions;
using GameServer.Source.Handlers;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using System;

namespace GameServer.Source.Models
{
    public class ClientController
    {
        private static readonly ILog Logger = LogManager.GetLogger<ClientController>();

        public string UserId { get; set; }
        public string Username { get; set; }
        public string SessionId { get; set; }
        public NetworkStream Stream { get; set; }
        readonly TickBasedScheduler _scheduler;

        public DateTime? LastInput { get; set; }

        public ClientController (string userId, string username, string sessionId, NetworkStream networkStream, TickBasedScheduler tickBasedScheduler)
        {
            var user = FirebaseService.AddOrRetrieveUser(userId, username).Result;
            UserId = user.UserId;
            Username = user.Username;

            SessionId = sessionId;
            Stream = networkStream;
            _scheduler = tickBasedScheduler;
        }

        public async Task HandleConnection()
        {
            var rateLimiter = new RateLimiter(threshold: AppSettings.GetValue<int>("Server:TickRate"), TimeSpan.FromSeconds(1));
            Logger.Info($"User connected: {UserId}");

            while (true)
            {
                try
                {
                    if (!rateLimiter.CheckLimit()) break;

                    byte[] message = new byte[4096];
                    var bytesRead = await Stream.ReadAsync(message.AsMemory(0, 4096));
                    if (bytesRead == 0) { Logger.Info($"User disconnected: {UserId}"); break; }

                    ServerRequest request = SocketIO.ReadAndDeserialize<ServerRequest>(Encoding.ASCII.GetString(message, 0, bytesRead));
                    if (request.SessionId != SessionId) { throw new BadSessionException("Unexpected session token"); }
                    Logger.Info($"Request received {JsonConvert.SerializeObject(request)}");

                    LastInput = DateTime.UtcNow;
                    if (request.Request is IRealtimeRequest)
                    {
                        IRealtimeHandler handler = RealtimeHandlerFactory.GetHandler(request);
                        //handler.HandleRequest();
                    }
                    else if (request.Request is ITickBasedRequest)
                    {
                        _scheduler.EnqueueInput(request);
                    }
                    else
                    {
                        throw new UnsupportedRequestTypeException("Request was not Realtime or TickBased");
                    }
                }
                catch(Exception ex)
                {
                    Logger.Error(ex);
                    break;
                }
            }
        }
    }
}
