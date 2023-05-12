using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace GameServer.Source.Exceptions
{
    public class UnsupportedRequestTypeException : Exception
    {
        public UnsupportedRequestTypeException(string? message) : base(message)
        {
            
        }
    }
}
