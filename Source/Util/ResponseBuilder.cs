using GameLibrary.Request;
using GameLibrary.Response;
using GameLibrary.Response.Util;

namespace GameServer.Source.Util
{
    public static class ResponseBuilder
    {
        public static ServerResponse CreateErrorResponse(ServerRequest request, string message)
        {
            var RS = new ServerResponse(
                    request.CorrelationId,
                    new ErrorResponse(ResponseStatus.OK, message)
                );
            return RS;
        }

        public static ServerResponse CreateErrorResponse(string message)
        {
            var RS = new ServerResponse(
                    Guid.NewGuid(),
                    new ErrorResponse(ResponseStatus.OK, message)
                );
            return RS;
        }
    }
}
