using System.Runtime.CompilerServices;

namespace GameServer.Source.Exceptions
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException(string? message) : base(message)
        {

        }
    }
}
