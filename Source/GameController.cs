﻿using Common.Logging;
using GameLibrary;
using GameLibrary.Request;
using GameServer.Source.Exceptions;
using GameServer.Source.Models;
using GameServer.Source.Services;
using GameServer.Source.Util;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace GameServer.Source
{
    public sealed class GameController
    {
        private static readonly ILog Logger = LogManager.GetLogger<ConnectedUser>();

        readonly TcpListener tcpListener;
        Thread listenerThread;
        readonly RequestScheduler scheduler;
        readonly int port = AppSettings.GetValue<Int32>("Server:Port");

        long numConnections;
        bool isListening = false;
        readonly long maxConnections = AppSettings.GetValue<Int32>("Server:MaxConnections");

        public GameController(RequestScheduler inputScheduler)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            scheduler = inputScheduler;
            Logger.Info($"{GetType().Name} has finished constructing.");
        }

        public void Start()
        {
            if(listenerThread == null)
            {
                isListening = true;
                listenerThread = new Thread(new ThreadStart(ListenForClients));
                listenerThread.Start();
                Logger.Info($"{GetType().Name} has started on port {port}.");
            } else
            {
                Logger.Warn($"{GetType().Name} is already running.");
            }
        }

        public void Stop()
        {
            if(listenerThread != null)
            {
                isListening = false;
                listenerThread.Join();
                Logger.Info($"{GetType().Name} has stopped listening.");
            }
        }

        public void ListenForClients()
        {
            tcpListener.Start();

            while (isListening)
            {
                TcpClient client = tcpListener.AcceptTcpClient();

                try
                {
                    VerifyCapacity();
                    Thread clientThread = new(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    var errorResponse = ResponseBuilder.CreateErrorResponse(ex.Message);
                    client.GetStream().Write(SocketIO.ObjectToByteArray(errorResponse));
                    client.Close();
                }
            }

            tcpListener.Stop();
        }

        public async void HandleClientComm(object? client)
        {
            // Validate Client
            if (client == null) return;
            TcpClient tcpClient = (TcpClient)client;
            var greeting = ReadGreeting(tcpClient);
            var token = await FirebaseService.ValidateSessionTokenAsync(greeting.SessionId);
            
            // Add Client to connections
            Interlocked.Increment(ref numConnections); // TODO this needs to happen atomically with the capacity check
            Logger.Info($"Client connected from: {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address}");
            
            // Build ConnectedUser object and start reading from the client
            NetworkStream clientStream = tcpClient.GetStream();
            var user = new ConnectedUser(token.Uid, greeting.SessionId);
            user.HandleConnection(clientStream, scheduler);

            // Cleanup
            Logger.Info($"Client {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address} disconnected.");
            tcpClient.Close();
            Interlocked.Decrement(ref numConnections);
        }

        public GreetingRequest ReadGreeting(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();

            byte[] greetingMessage = new byte[4096];
            int greetingBytesRead = clientStream.Read(greetingMessage, 0, 4096);
            if (greetingBytesRead == 0){ /*Client disconnected */ }

            var message = Encoding.ASCII.GetString(greetingMessage, 0, greetingBytesRead);
            Logger.Info($"Received greeting: {message}");
            IRequest request = SocketIO.ReadAndDeserialize<IRequest>(message);

            if (request is GreetingRequest)
            {
                return (GreetingRequest)request;
            }
            throw new BadGreetingException($"Request was not of type GreetingRequest.");
        }

        public void VerifyCapacity()
        {
            // Check if server has room
            if (Interlocked.Read(ref numConnections) >= maxConnections) throw new Exception("Server is full");
        }
    }
}
