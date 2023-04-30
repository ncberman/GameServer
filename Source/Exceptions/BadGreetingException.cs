using System.Runtime.CompilerServices;

namespace GameServer.Source.Exceptions
{
    public class BadGreetingException : Exception
    {
        public BadGreetingException(string? message) : base(message)
        {
            
        }
    }
}
