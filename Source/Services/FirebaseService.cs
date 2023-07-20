using FirebaseAdmin.Auth;
using GameLibrary;
using GameServer.Source.Exceptions;
using Common.Logging;
using GameServer.Source.Models.Database;
using GameServer.Source.Util;

namespace GameServer.Source.Services
{
    public class FirebaseService
    {
        private static readonly ILog Logger = LogManager.GetLogger<FirebaseService>();

        public static async Task<FirebaseToken> ValidateSessionTokenAsync(string sessionId)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(sessionId.ToString());
                Logger.Info($"User verified: {decodedToken.Uid}");
                return decodedToken;
            }
            catch (FirebaseAuthException ex)
            {
                throw new BadSessionException($"{ex.Message}");
            }

            throw new BadSessionException($"There was an error validating session token.");
        }

        public static async Task<T> RetrieveData<T>(string path)
        {
            FirebaseRealtimeDatabase realtimeDatabase = new();
            var data = await realtimeDatabase.GetDataAsync<T>(path);
            return data == null ? throw new DataNotFoundException($"No object of type {typeof(T)} at {path}") : data;
        }

        public static async Task<DatabaseUser> AddOrRetrieveUser(string userId, string username)
        {
            var realtimeDatabase = new FirebaseRealtimeDatabase();
            var user = await realtimeDatabase.GetDataAsync<DatabaseUser>($"users/user_{userId}");
            if (user == null)
            {
                user = new()
                {
                    UserId = userId,
                    Username = username,
                    CharacterLimit = 1
                };
                await realtimeDatabase.AddDataAsync(string.Format(Constants.USER_DIR, userId), user);
            }
            return user;
        }
    }
}
