using GameLibrary.Request.Util;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Source.Util
{
    public static class SocketIO
    {
        public static T ReadAndDeserialize<T>(string message)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new RequestConverter());
            T? input = JsonConvert.DeserializeObject<T>(message, settings);
            return input == null ? throw new Exception() : input;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Encoding.ASCII.GetBytes(json);
        }
    }
}
