using GameLibrary.Request;
using GameLibrary.Response;
using GameServer.Source.Models;

namespace GameServer.Source.Handlers.Realtime
{
    public class ChatHandler : IRealtimeHandler
    {
        public Task<ServerResponse> HandleRequest(ServerRequest request, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
