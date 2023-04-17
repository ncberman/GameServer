using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Newtonsoft.Json;

namespace GameServer.Source
{
    public sealed class GameController
    {
        readonly ILogger<GameController> logger;

        private readonly object connectionLock = new object();

        readonly TcpListener tcpListener;
        readonly Thread listenerThread;
        readonly InputScheduler scheduler;
        readonly int port = AppSettings.GetValue<Int32>("Server:Port");

        int numConnections;
        readonly int maxConnections = AppSettings.GetValue<Int32>("Server:MaxConnections");

        public GameController(ILogger<GameController> logger, InputScheduler inputScheduler)
        {
            this.logger = logger;
            tcpListener = new TcpListener(IPAddress.Any, port);
            scheduler = inputScheduler;
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.Start();

            logger.LogInformation($"GameController has finished constructing.");
        }

        private void ListenForClients()
        {
            tcpListener.Start();
            logger.LogInformation($"Client Listener started on port {port}");

            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                lock(connectionLock)
                {
                    if (numConnections >= maxConnections) continue;
                    else numConnections++;
                }
                logger.LogInformation($"Client connected from: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                Thread clientThread = new (new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object? client)
        {
            if (client == null) return;
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            while (true)
            {
                byte[] message = new byte[4096];
                int bytesRead;

                try
                {
                    // Read the incoming message from the client
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception while reading from client stream: {ex.Message}");
                    break;
                }

                if (bytesRead == 0)
                {
                    // Client disconnected
                    break;
                }

                // Convert the byte array into a string
                string data = Encoding.ASCII.GetString(message, 0, bytesRead);
                logger.LogInformation($"Received data: {data}");

                // Deserialize the received message into an object
                object? input = JsonConvert.DeserializeObject<object>(data);
                if (input == null) { continue; }

                // Add the input to the scheduler
                scheduler.EnqueueInput(input);
            }

            tcpClient.Close();
            lock (connectionLock)
            {
                numConnections--;
            }
        }

    }
}
