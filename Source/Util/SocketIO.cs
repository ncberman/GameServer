using Newtonsoft.Json;
using System.Text;

namespace GameServer.Source.Util
{
    public static class SocketIO
    {
        public static T ReadAndDeserialize<T>(string message)
        {
            T? input = JsonConvert.DeserializeObject<T>(message);
            if (input == null) { throw new Exception(); }
            return input;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Encoding.ASCII.GetBytes(json);
        }
    }
}
