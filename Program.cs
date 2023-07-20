using Common.Logging.Serilog;
using Common.Logging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Serilog;
using GameServer.Source.Services;
using GameServer.Source.Models.Database;
using Firebase.Database;
using GameServer.Source.Components;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateLogger();

        var factoryAdapter = new SerilogFactoryAdapter(Log.Logger);
        LogManager.Adapter = factoryAdapter;


        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ServerController>();
                services.AddSingleton<PlayerManager>();
                services.AddSingleton<TickBasedHandler>();
                services.AddSingleton<TickBasedScheduler>();
            });

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("/app/TribalGamesAuth.secret"),
        });

        var host = builder.Build();
        var serverController = host.Services.GetService<ServerController>();
        var tickRequestScheduler = host.Services.GetService<TickBasedScheduler>();
        var tickRequestHandler = host.Services.GetService<TickBasedHandler>();

        // TODO fail program if any of these are null
        tickRequestScheduler?.EnableDiagnostics();
        tickRequestHandler?.Start();
        serverController?.Start();
    }

    //public static async void DoSomething()
    //{
    //    var db = new FirebaseRealtimeDatabase();
    //    var user = new DatabaseUser();
    //    user.Username = "Test";
    //    user.UserId = Guid.NewGuid().ToString();
    //    await db.AddDataAsync("users/4b4f8840-ba09-4eb1-87c3-82d09262601e/characters", "WarriorGuy", Guid.NewGuid().ToString());
    //}
}