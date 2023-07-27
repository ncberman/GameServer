using Common.Logging;
using GameLibrary;
using GameLibrary.Request;
using GameServer.Source.Models;

namespace GameServer.Source.Components
{
    public sealed class TickBasedScheduler
    {
        private static readonly ILog Logger = LogManager.GetLogger<ClientController>();

        readonly Queue<ServerRequest> requestQueue;
        readonly int diagnosticInterval;
        int requestCount;

        readonly object queueLock = new();

        public TickBasedScheduler()
        {
            requestQueue = new Queue<ServerRequest>();
            diagnosticInterval = 60;
            Logger.Info($"{GetType().Name} constructed");
        }

        public void EnableDiagnostics()
        {
            var diagnosticTimer = new Timer(ReportDiagnostics, null, TimeSpan.Zero, TimeSpan.FromSeconds(diagnosticInterval));
            Logger.Info($"{GetType().Name} diagnostics enabled.");
        }

        private void ReportDiagnostics(object? state)
        {
            lock (queueLock)
            {
                Logger.Info($"Received {requestCount} inputs in the last {diagnosticInterval}s.");
                requestCount = 0;
            }
        }

        public void EnqueueInput(ServerRequest input)
        {
            lock (queueLock)
            {
                requestQueue.Enqueue(input);
                requestCount++;
            }
        }

        public Queue<ServerRequest> DequeueInput()
        {
            lock (queueLock)
            {
                var returnQueue = requestQueue;
                requestQueue.Clear();
                return returnQueue;
            }
        }

        public int GetInputQueueCount()
        {
            lock (queueLock)
            {
                return requestQueue.Count;
            }
        }
    }
}
