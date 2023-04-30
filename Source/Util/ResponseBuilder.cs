using GameLibrary;

namespace GameServer.Source.Util
{
    public static class ResponseBuilder
    {
        public static ServerResponse CreateErrorResponse(string message)
        {
            var RS = new ServerResponse();
            RS.ResponseTypeEnum = ResponseTypeEnum.ERROR;
            var errRS = new ErrorResponse();
            errRS.Message = message;
            RS.Response = errRS;
            return RS;
        }
    }
}
