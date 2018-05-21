using Microsoft.Extensions.DependencyInjection;
using MTD.Haven.Dals;
using MTD.Haven.Dals.Implementation;
using MTD.Haven.Domain;
using MTD.Haven.Managers;
using MTD.Haven.Managers.Implementation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MTD.Haven.Server
{
    internal class Program
    {
        private const int PortNumber = 4000;
        private const int BacklogSize = 20;

        //private static readonly IPAddress LocalAddr = IPAddress.Parse("100.104.10.17");
        private static readonly IPAddress LocalAddr = IPAddress.Parse("127.0.0.1");
        private static TcpListener _server;

        public List<Connection> Connections = new List<Connection>();

        private static void Main() => new Program().Start();

        public void Start()
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton<IPlayerDal, PlayerDal>()
            .AddSingleton<IPlayerManager, PlayerManager>()
            .AddSingleton(Connections)
            .BuildServiceProvider();

            try
            {
                _server = new TcpListener(LocalAddr, PortNumber);
                _server.Start(BacklogSize);

                new Thread(ServerLoop).Start();

                while (true)
                {
                    Console.WriteLine("Waiting for a connection... ");
                    var client = _server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // ReSharper disable once ObjectCreationAsStatement
                    new Connection(client, serviceProvider.GetRequiredService<IPlayerManager>(), Connections);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                _server.Stop();
            }
        }

        private void ServerLoop()
        {
            var minute = 0;
            var half = 0;
            var hour = 0;

            while (true)
            {
                minute += Constants.PulseTimer;
                half += Constants.PulseTimer;
                hour += Constants.PulseTimer;

                if (minute >= Constants.Minute)
                {
                    minute = 0;

                    foreach (var connection in Connections)
                    {
                        connection.Writer.WriteLine($"{DateTime.UtcNow} - Another minute has passed.");
                        connection.Writer.Flush();
                    }
                }

                if (half >= Constants.Half)
                {
                    half = 0;

                    foreach (var connection in Connections)
                    {
                        connection.Writer.WriteLine($"{DateTime.UtcNow} - Another half hour has passed.");
                        connection.Writer.Flush();
                    }
                }

                if (hour >= Constants.Hour)
                {
                    hour = 0;

                    foreach (var connection in Connections)
                    {
                        connection.Writer.WriteLine($"{DateTime.UtcNow} - Another hour has passed.");
                        connection.Writer.Flush();
                    }
                }
                Console.WriteLine(minute);
                Thread.Sleep(Constants.PulseTimer);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}


