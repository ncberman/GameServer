using Common.Logging;
using GameServer.Source.Models;
using GameServer.Source.Util;

namespace GameServer.Source.Components
{
    public sealed class TickBasedHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<TickBasedHandler>();

        readonly TickBasedScheduler scheduler;

        public TickBasedHandler(TickBasedScheduler inputScheduler)
        {
            scheduler = inputScheduler;
            Logger.Info($"{GetType().Name} constructed");
        }

        public void Start()
        {
            var processThread = new Thread(new ThreadStart(StartProcessing));
            processThread.Start();
            Logger.Info($"{GetType().Name} has started processing inputs.");
        }

        // Starts a timer using the tickrate defined in the settings to process the inputs from the RequestScheduler
        void StartProcessing()
        {
            var tickRate = 1 / AppSettings.GetValue<double>("Server:TickRate");
            var tickTimer = new Timer(ProcessInputs, null, TimeSpan.Zero, TimeSpan.FromSeconds(tickRate));
        }

        private void ProcessInputs(object? state)
        {
            var tickInput = scheduler.DequeueInput();

            // Handle inputs
        }
    }
}