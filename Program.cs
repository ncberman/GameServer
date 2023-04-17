using GameServer.Source;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging();
                services.AddSingleton<GameController>();
                services.AddSingleton<InputHandler>();
                services.AddSingleton<InputScheduler>();
            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {
                loggingBuilder.AddConsole();
            });

        var host = builder.Build();
        var gameController = host.Services.GetService<GameController>();
        var inputHandler = host.Services.GetService<InputHandler>();
    }
}