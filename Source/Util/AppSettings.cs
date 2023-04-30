namespace GameServer.Source.Util
{
    public static class AppSettings
    {
        private static readonly IConfiguration _configuration;

        static AppSettings()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static T GetValue<T>(string key)
        {
            var value = _configuration[key];
            if (value == null)
            {
                throw new KeyNotFoundException($"Key '{key}' not found in appsettings.json");
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
