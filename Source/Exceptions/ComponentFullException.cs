using System.Runtime.CompilerServices;

namespace GameServer.Source.Exceptions
{
    public class ComponentFullException : Exception
    {
        public ComponentFullException(string? message) : base(message)
        {
            
        }
    }
}
