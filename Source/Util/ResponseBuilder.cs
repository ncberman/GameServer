using GameLibrary.Response;

namespace GameServer.Source.Util
{
    public static class ResponseBuilder
    {
        public static ServerResponse CreateErrorResponse(string message)
        {
            var RS = new ServerResponse(
                    new ErrorResponse(message)
                );
            return RS;
        }
    }
}
