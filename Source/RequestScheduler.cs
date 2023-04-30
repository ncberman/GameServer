using Common.Logging;
using GameLibrary;
using GameLibrary.Request;
using GameServer.Source.Models;

namespace GameServer.Source
{
    public sealed class RequestScheduler
    {
        private static readonly ILog Logger = LogManager.GetLogger<ConnectedUser>();

        readonly Queue<IRequest> requestQueue;
        readonly int diagnosticInterval;
        int requestCount;

        readonly object queueLock = new object();

        public RequestScheduler()
        {
            requestQueue = new Queue<IRequest>();
            diagnosticInterval = 60;
            Logger.Info($"{GetType().Name} has finished constructing.");
        }

        public void EnableDiagnostics()
        {
            var diagnosticTimer = new Timer(ReportDiagnostics, null, TimeSpan.Zero, TimeSpan.FromSeconds(diagnosticInterval));
            Logger.Info($"{GetType().Name} diagnostics enabled.");
        }

        private void ReportDiagnostics(object? state)
        {
            lock(queueLock)
            {
                Logger.Info($"Received {requestCount} inputs in the last {diagnosticInterval}s.");
                requestCount = 0;
            }
        }

        public void EnqueueInput(IRequest input)
        {
            lock (queueLock)
            {
                requestQueue.Enqueue(input);
                requestCount++;
            }
        }

        public Queue<IRequest> DequeueInput()
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
