using GameLibrary.Model;
using GameLibrary.Request;
using GameLibrary.Response;
using GameLibrary.Response.Util;
using GameServer.Source.Models;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;

namespace GameServer.Source.Handlers.Realtime
{
    public class RetrieveCharacterHandler : IRealtimeHandler
    {
        public async Task<ServerResponse> HandleRequest(ServerRequest request, string userId)
        {
            try
            {
                // TODO close db lock

                var retrieveCharacter = (RetrieveCharacterRequest)request.Request;
                var user = FirebaseService.RetrieveData<DatabaseUser>(string.Format(Constants.USER_DIR, userId)).Result;

                if (!user.Characters.Contains(retrieveCharacter.CharacterName)) 
                { 
                    return ResponseBuilder.CreateErrorResponse(request, $"{user.Username} does not have character with name {retrieveCharacter.CharacterName}"); 
                }

                var character = await FirebaseService.RetrieveData<SharedCharacter>(string.Format(Constants.CHARACTER_DIR, retrieveCharacter.CharacterName));
                if(character == null)
                {
                    return ResponseBuilder.CreateErrorResponse(request, $"No character exists with name {retrieveCharacter.CharacterName}");
                }

                var retrieveCharacterResponse = new RetrieveCharacterResponse(ResponseStatus.OK, $"{retrieveCharacter.CharacterName} retrieved successfully.", character);
                var serverResponse = new ServerResponse(request.CorrelationId, retrieveCharacterResponse);

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
