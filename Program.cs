using Common.Logging.Serilog;
using Common.Logging;
using FirebaseAdmin;
using GameServer.Source;
using Google.Apis.Auth.OAuth2;
using Serilog;
using GameServer.Source.Services;
using GameServer.Source.Models.Database;
using Firebase.Database;

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
                services.AddSingleton<GameController>();
                services.AddSingleton<RequestHandler>();
                services.AddSingleton<RequestScheduler>();
            });

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("/app/keyfile.json"),
        });

        var host = builder.Build();
        var gameController = host.Services.GetService<GameController>();
        var requestScheduler = host.Services.GetService<RequestScheduler>();
        var requestHandler = host.Services.GetService<RequestHandler>();

        // TODO fail program if any of these are null
        requestScheduler?.EnableDiagnostics();
        requestHandler?.Start();
        gameController?.Start();

        DoSomething();
        Thread.Sleep(10000);
    }

    public static async void DoSomething()
    {
        var db = new FirebaseRealtimeDatabase();
        var user = new User();
        user.UserName = "Test";
        user.ClientId = Guid.NewGuid().ToString();
        await db.AddDataAsync("users/4b4f8840-ba09-4eb1-87c3-82d09262601e/characters", "WarriorGuy", Guid.NewGuid().ToString());
    }
}