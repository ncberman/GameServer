using GameLibrary.Request;
using GameServer.Source.Exceptions;
using GameServer.Source.Handlers.Realtime;
using GameServer.Source.Models;

namespace GameServer.Source.Handlers
{
    public static class RealtimeHandlerFactory
    {
        public static IRealtimeHandler GetHandler(ServerRequest request)
        {
            switch (request.Request.RequestType)
            {
                case RequestType.CHARACTER_CREATE:
                    return new RetrieveCharacterHandler();

                case RequestType.CHAT:
                    return new ChatHandler();

                default:
                    throw new UnsupportedRequestTypeException(string.Format("RealtimeHandlerFactory can't handle request of type: {0}", request.Request.RequestType.ToString()));
            }
        }
    }
}