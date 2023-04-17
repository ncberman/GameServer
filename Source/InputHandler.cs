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
            ProcessInputs();
        }

        private void ProcessInputs()
        {
            DateTime lastTick = DateTime.Now;
            double tickRate = AppSettings.GetValue<Double>("Server:TickRate");

            while (true)
            {
                // Check if the next tick should occur
                if(DateTime.Now.Subtract(lastTick).TotalSeconds > (1/tickRate)) {
                    lastTick = DateTime.Now;

                    var tickInput = scheduler.DequeueInput();
                    foreach(var input in tickInput )
                    {
                        // Handle each input
                    }
                }
            }
        }
    }
}