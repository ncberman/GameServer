using GameLibrary.Model;
using GameLibrary.Request;
using GameLibrary.Response;
using GameLibrary.Response.Util;
using GameServer.Source.Exceptions;
using GameServer.Source.Models;
using GameServer.Source.Models.Database;
using GameServer.Source.Services;
using GameServer.Source.Util;

namespace GameServer.Source.Handlers.Realtime
{
    public class CreateCharacterHandler : IRealtimeHandler
    {
        public async Task<ServerResponse> HandleRequest(ServerRequest request, string userId)
        {
            /*
                We want to check if a character can be created before we allow this request
                A character is allowed to be created if it meets all the following criteria.

                a. Creating the character would not allow the user to go over their allowed number of characters
                b. The name is not already in use by another character
                c. The name follows our naming guidelines
                d. The characteristics of the character are a valid combination
            */

            try
            {
                // TODO close db lock

                var CreateRequest = (CreateCharacterRequest)request.Request;
                var user = FirebaseService.RetrieveData<DatabaseUser>(string.Format(Constants.USER_DIR, userId)).Result;

                if (/* a */!DoesUserHaveEmptyCharacterSlot(user)) { return ResponseBuilder.CreateErrorResponse(request, "User has no available character slots"); }
                else if (/* b */IsNameInUse(CreateRequest.CharacterName)) { return ResponseBuilder.CreateErrorResponse(request, "Name is not available"); }
                else if (/* c */IsNameInappropriate(CreateRequest.CharacterName)) { return ResponseBuilder.CreateErrorResponse(request, "Name does not follow our guidelines"); }
                else if (/* d */!IsValidCharacter(CreateRequest)) { return ResponseBuilder.CreateErrorResponse(request, "Character is not a valid character combo"); }

                // Create the new character
                user.Characters.Add(CreateRequest.CharacterName);
                var newChar = new SharedCharacter(CreateRequest.CharacterName, 1);

                // Add the character to the database, update the user
                var database = new FirebaseRealtimeDatabase();
                var addCharacterTask = database.AddDataAsync(string.Format(Constants.CHARACTER_DIR, CreateRequest.CharacterName), newChar);
                var updateUserTask = database.UpdateDataAsync(string.Format(Constants.USER_DIR, userId), user);

                await Task.WhenAll(addCharacterTask, updateUserTask);

                // Verify that the changes occurred
                var newCharacter = FirebaseService.RetrieveData<SharedCharacter>(string.Format(Constants.CHARACTER_DIR, CreateRequest.CharacterName));
                var updatedUser = FirebaseService.RetrieveData<DatabaseUser>(string.Format(Constants.USER_DIR, userId));

                await Task.WhenAll(addCharacterTask, updateUserTask);

                // TODO validate 
                if (!updatedUser.Result.Characters.Contains(CreateRequest.CharacterName)) { return ResponseBuilder.CreateErrorResponse(request, "Something failed with character creation."); }

                var createCharacterResponse = new CreateCharacterResponse(ResponseStatus.OK, $"{CreateRequest.CharacterName} created successfully.", newCharacter.Result);
                var serverResponse = new ServerResponse(request.CorrelationId, createCharacterResponse);

                // Open db lock
                return serverResponse;
            }
            catch
            {
                // Open db lock
                throw;
            }
        }

        private static bool IsValidCharacter(CreateCharacterRequest characterRequest)
        {
            return true;
        }

        private static bool IsNameInUse(string charName)
        {
            try
            {
                _ = FirebaseService.RetrieveData<SharedCharacter>(string.Format(Constants.CHARACTER_DIR, charName)).Result;
            }
            catch (AggregateException e)
            {
                if (e.InnerException?.GetType() == typeof(DataNotFoundException))
                    return false;
                else throw e;
            }
            return true;
        }

        private static bool IsNameInappropriate(string charName)
        {
            if (charName.Length < 3) return true;
            else return false;
        }

        private static bool DoesUserHaveEmptyCharacterSlot(DatabaseUser user)
        {
            return user.Characters.Count < user.CharacterLimit;
        }
    }
}
