namespace GameServer.Source
{
    public sealed class InputHandler
    {
        readonly ILogger<InputHandler> logger;

        InputScheduler scheduler;

        public InputHandler(ILogger<InputHandler> logger, InputScheduler inputScheduler)
        {
            this.logger = logger;
            scheduler = inputScheduler;
            logger.LogInformation($"InputHandler has finished constructing.");
            var processThread = new Thread(new ThreadStart(StartProcessing));
            processThread.Start();
        }

        // Starts a timer using the tickrate defined in the settings to process the inputs from the InputScheduler
        void StartProcessing()
        {
            var tickRate = (1/AppSettings.GetValue<double>("Server:TickRate"));
            var tickTimer = new Timer(ProcessInputs, null, TimeSpan.Zero, TimeSpan.FromSeconds(tickRate));
        }

        private void ProcessInputs(object? state)
        {
            var tickInput = scheduler.DequeueInput();

            // Handle inputs
        }
    }
}