using Common.Logging;
using GameLibrary.Request;
using GameLibrary.Response;
using GameServer.Source.Exceptions;
using GameServer.Source.Models;
using GameServer.Source.Services;
using GameServer.Source.Util;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServer.Source.Components
{
    /*
     * The Server Controller is a component that accepts / rejects incoming socket connections based on a few properties
     *      - Is the server full? Reject
     *      - Did the connection send a valid greeting? Accept
     *      
     * After the controller accepts a connection it passes the connection off to the Player Manager
     */

    public sealed class ServerController
    {
        private static readonly ILog Logger = LogManager.GetLogger<ClientController>();

        readonly TcpListener tcpListener;
        Thread? listenerThread = null;
        readonly int port = AppSettings.GetValue<int>("Server:Port");
        readonly PlayerManager playerManager;
        readonly TickBasedScheduler scheduler;

        long numConnections;
        bool isListening = false;
        readonly long maxConnections = AppSettings.GetValue<int>("Server:MaxConnections");

        #region Manage
        public ServerController(TickBasedScheduler tickBasedScheduler)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            playerManager = PlayerManager.Instance;
            scheduler = tickBasedScheduler;
            Logger.Info($"{GetType().Name} constructed");
        }

        public void Start()
        {
            if (listenerThread == null)
            {
                isListening = true;
                listenerThread = new Thread(new ThreadStart(ListenForClients));
                listenerThread.Start();
                Logger.Info($"{GetType().Name} has started on port {port}.");
            }
            else
            {
                Logger.Warn($"{GetType().Name} is already running.");
            }
        }

        public void Stop()
        {
            if (listenerThread != null)
            {
                isListening = false;
                listenerThread.Join();
                Logger.Info($"{GetType().Name} has stopped listening.");
            }
        }
        #endregion

        public async void ListenForClients()
        {
            tcpListener.Start();

            while (isListening)
            {
                var client = tcpListener.AcceptTcpClient();
                if (await IsConnectionValid(client))
                {
                    Thread clientThread = new(new ParameterizedThreadStart(ValidateAndHandleClient));
                    clientThread.Start(client);
                }
            }

            tcpListener.Stop();
        }

        public async void ValidateAndHandleClient(object? client)
        {
            // Validate Client
            if (client == null) return;
            var tcpClient = (TcpClient)client;
            ServerRequest greeting = new(Guid.NewGuid(), "", new GreetingRequest(""));
            try
            {
                greeting = await ReadGreeting(tcpClient);
            }
            catch (Exception ex)
            {
                var errRS = ResponseBuilder.CreateErrorResponse(greeting, ex.Message);
                tcpClient.GetStream().Write(SocketIO.ObjectToByteArray(errRS));
                tcpClient.Close();
                return;
            }
            var token = await FirebaseService.ValidateSessionTokenAsync(greeting.SessionId);

            // Add Client to connections
            Interlocked.Increment(ref numConnections); // TODO this needs to happen atomically with the capacity check
            //string address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint)?.Address.ToString();
            //Logger.Info($"Client connected from: {address}");

            // Build ConnectedUser object and start reading from the client
            NetworkStream clientStream = tcpClient.GetStream();
            //user.HandleConnection(clientStream, scheduler);
            var clientController = new ClientController(
                    token.Uid, 
                    ((GreetingRequest)greeting.Request).Username,
                    greeting.SessionId,
                    clientStream,
                    scheduler
                );
            await playerManager.AddNewConnection(clientController.UserId, clientController);

            // Cleanup
            //Logger.Info($"Client {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address} disconnected.");
            tcpClient.Close();
            Interlocked.Decrement(ref numConnections);
        }

        #region Validation
        public async Task<bool> IsConnectionValid(TcpClient client)
        {
            if (IsServerFull())
            {
                await ReturnErrorAndClose(client, "Server is full.");
                return false;
            }
            return true;
        }

        public async Task<ServerRequest> ReadGreeting(TcpClient client)
        {
            //NetworkStream clientStream = client.GetStream();

            byte[] greetingMessage = new byte[4096];
            int greetingBytesRead = await client.GetStream().ReadAsync(greetingMessage);
            if (greetingBytesRead == 0) { /* Client disconnected */ }

            var message = Encoding.ASCII.GetString(greetingMessage, 0, greetingBytesRead);
            Logger.Info($"Received greeting: {message}");
            ServerRequest request = SocketIO.ReadAndDeserialize<ServerRequest>(message);

            if (request.Request is GreetingRequest)
            {
                var greetingResponse = new ServerResponse(request.CorrelationId, new GreetingResponse("Greeting Accepted!"));
                await client.GetStream().WriteAsync(SocketIO.ObjectToByteArray(greetingResponse));
                return request;
            }

            throw new BadGreetingException($"Request was not of type GreetingRequest.");
        }

        public bool IsServerFull()
        {
            // Check if server has room
            if (Interlocked.Read(ref numConnections) >= maxConnections) return true;
            return false;
        }

        private async static Task ReturnErrorAndClose(TcpClient client, string message)
        {
            var errorResponse = ResponseBuilder.CreateErrorResponse(message);
            await client.GetStream().WriteAsync(SocketIO.ObjectToByteArray(errorResponse));
            client.Close();
        }
        #endregion
    }
}
