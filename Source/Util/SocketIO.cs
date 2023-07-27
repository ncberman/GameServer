using GameLibrary.Request.Util;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Source.Util
{
    public static class SocketIO
    {
        public static readonly JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static T ReadAndDeserialize<T>(string message)
        {
            T? input = JsonConvert.DeserializeObject<T>(message, settings);
            return input == null ? throw new Exception() : input;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, settings);
            return Encoding.ASCII.GetBytes(json);
        }
    }
}
