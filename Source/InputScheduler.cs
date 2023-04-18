using GameLibrary;

namespace GameServer.Source
{
    public sealed class InputScheduler
    {
        readonly ILogger<InputScheduler> logger;

        Queue<InputObject> inputQueue;
        int diagnosticInterval;
        int inputCount;

        readonly object queueLock = new object();

        public InputScheduler(ILogger<InputScheduler> logger)
        {
            this.logger = logger;
            inputQueue = new Queue<InputObject>();
            diagnosticInterval = 60;
            var diagnosticTimer = new Timer(ReportDiagnostics, null, TimeSpan.Zero, TimeSpan.FromSeconds(diagnosticInterval));
            logger.LogInformation($"InputScheduler has finished constructing.");
        }

        private void ReportDiagnostics(object? state)
        {
            lock(queueLock)
            {
                logger.LogInformation($"Received {inputCount} inputs in the last {diagnosticInterval}s.");
                inputCount = 0;
            }
        }

        public void EnqueueInput(InputObject input)
        {
            lock (queueLock)
            {
                inputQueue.Enqueue(input);
                inputCount++;
            }
        }

        public Queue<InputObject> DequeueInput()
        {
            lock (queueLock)
            {
                var returnQueue = inputQueue;
                inputQueue.Clear();
                return returnQueue;
            }
        }

        public int GetInputQueueCount()
        {
            lock (queueLock)
            {
                return inputQueue.Count;
            }
        }
    }
}
