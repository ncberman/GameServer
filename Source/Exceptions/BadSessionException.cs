using System.Runtime.CompilerServices;

namespace GameServer.Source.Exceptions
{
    public class BadSessionException : Exception
    {
        public BadSessionException(string? message) : base(message)
        {

        }
    }
}
