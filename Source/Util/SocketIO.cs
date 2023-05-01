using GameLibrary.Request.Util;
using Newtonsoft.Json;
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
