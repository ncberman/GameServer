using GameLibrary.Response;
using GameLibrary.Response.Util;

namespace GameServer.Source.Util
{
    public static class ResponseBuilder
    {
        public static ServerResponse CreateErrorResponse(string message)
        {
            var RS = new ServerResponse(
                    new ErrorResponse(ResponseStatus.OK, message)
                );
            return RS;
        }
    }
}
