namespace GameServer.Source
{
    public sealed class InputScheduler
    {
        readonly ILogger<InputScheduler> logger;

        // Queue to store incoming inputs
        private Queue<object> inputQueue;

        // Lock object for thread safety
        private readonly object queueLock = new object();

        // Private constructor to prevent external instantiation
        public InputScheduler(ILogger<InputScheduler> logger)
        {
            this.logger = logger;
            inputQueue = new Queue<object>();
            logger.LogInformation($"InputScheduler has finished constructing.");
        }

        // Enqueues a QueuedInput object onto the input queue
        public void EnqueueInput(object input)
        {
            lock (queueLock)
            {
                inputQueue.Enqueue(input);
            }
        }

        // Dequeues and returns the next input from the input queue
        public Queue<object> DequeueInput()
        {
            lock (queueLock)
            {
                var returnQueue = inputQueue;
                inputQueue.Clear();
                return returnQueue;
            }
        }

        // Returns the number of inputs currently in the input queue
        public int GetInputQueueCount()
        {
            lock (queueLock)
            {
                return inputQueue.Count;
            }
        }
    }
}
