using GameLibrary.Model;
using GameLibrary.Request;
using GameLibrary.Response;
using GameServer.Source.Exceptions;
using GameServer.Source.Models;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;

namespace GameServer.Source.Handlers.Realtime
{
    public class RetrieveUserHandler : IRealtimeHandler
    {
        public async Task<ServerResponse> HandleRequest(ServerRequest request, string userId)
        {
            /*
                Description here
            */

            try
            {
                // TODO close db lock

                var retrieveRequest = (RetrieveUserRequest)request.Request;
                var user = await FirebaseService.RetrieveData<DatabaseUser>(string.Format(Constants.USER_DIR, userId));

                var sharedUser = new SharedUser(user.Username, user.Characters);
                if(sharedUser == null)
                {
                    return ResponseBuilder.CreateErrorResponse(request, $"Could not find user {retrieveRequest.UserId}");
                }

                var retrieveUserResponse = new RetrieveUserResponse($"{retrieveRequest.UserId} retrieved successfully.", sharedUser);
                var serverResponse = new ServerResponse(request.CorrelationId, retrieveUserResponse);

                // Open db lock
                return serverResponse;
            }
            catch
            {
                // Open db lock
                throw;
            }
        }
    }
}
