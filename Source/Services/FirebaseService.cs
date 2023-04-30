using FirebaseAdmin.Auth;
using GameLibrary;
using GameServer.Source.Exceptions;
using Common.Logging;

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
    }
}
