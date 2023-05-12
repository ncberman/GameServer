using GameLibrary.Request;
using GameServer.Source.Exceptions;
using GameServer.Source.Models;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;

namespace GameServer.Source.Handlers.Realtime
{
    public class CharacterCreateHandler : IRealtimeHandler
    {
        public void HandleRequest(ServerRequest request, string userId)
        {
            /*
                We want to check if a character can be created before we allow this request
                A character is allowed to be created if it meets all the following criteria.

                - Creating the character would not allow the user to go over their allowed number of characters
                - The name is not already in use by another character
                - The name follows our naming guidelines
                - The characteristics of the character are a valid combination
            */
            var user = FirebaseService.RetrieveData<User>(string.Format(Constants.USER_DIR, userId)).Result;

            if (user.Characters.Length >= user.CharacterLimit) throw new DataNotFoundException("");


            throw new NotImplementedException();
        }
    }
}
