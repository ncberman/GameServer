using GameLibrary.Request;

namespace GameServer.Source.Models
{
    public interface IRealtimeHandler
    {
        void HandleRequest(ServerRequest request, string userId);
    }
}
