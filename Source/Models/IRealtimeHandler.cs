using GameLibrary.Request;
using GameLibrary.Response;

namespace GameServer.Source.Models
{
    public interface IRealtimeHandler
    {
        Task<ServerResponse> HandleRequest(ServerRequest request, string userId);
    }
}
