using FirebaseAdmin.Auth;
using GameLibrary;
using GameServer.Source.Exceptions;
using Common.Logging;
using GameServer.Source.Models.Database;

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
            if (data == null) throw new DataNotFoundException($"No object of type {typeof(T)} at {path}");
            return data;
        }

        public static async Task<User> AddOrRetrieveUser(string userId, string username)
        {
            var realtimeDatabase = new FirebaseRealtimeDatabase();
            var user = await realtimeDatabase.GetDataAsync<User>($"users/user_{userId}");
            if (user == null)
            {
                user = new()
                {
                    UserId = userId,
                    Username = username
                };
                await realtimeDatabase.AddDataAsync($"users/", $"user_{user.UserId}", user);
            }
            return user;
        }
    }
}
