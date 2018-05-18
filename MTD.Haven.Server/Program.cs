using Microsoft.Extensions.DependencyInjection;
using MTD.Haven.Dals;
using MTD.Haven.Dals.Implementation;
using MTD.Haven.Domain;
using MTD.Haven.Managers;
using MTD.Haven.Managers.Implementation;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MTD.Haven.Server
{
    class Program
    {
        private const int PortNumber = 4000;
        private const int BacklogSize = 20;

        static Connection _connection;

        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
            .AddSingleton<IPlayerDal, PlayerDal>()
            .AddSingleton<IPlayerManager, PlayerManager>()
            .BuildServiceProvider();

            Socket server = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
            server.Listen(BacklogSize);
            new Thread(ServerLoop).Start();

            while (true)
            {
                Socket conn = server.Accept();
                _connection = new Connection(conn, serviceProvider.GetRequiredService<IPlayerManager>());
            }
        }

        static void ServerLoop()
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

                    foreach (var connection in _connection._connections)
                    {
                        connection._writer.WriteLine($"{DateTime.UtcNow} - Another minute has passed.");
                        connection._writer.Flush();
                    }
                }

                if (half >= Constants.Half)
                {
                    half = 0;

                    foreach (var connection in _connection._connections)
                    {
                        connection._writer.WriteLine($"{DateTime.UtcNow} - Another half hour has passed.");
                        connection._writer.Flush();
                    }
                }

                if (hour >= Constants.Hour)
                {
                    hour = 0;

                    foreach (var connection in _connection._connections)
                    {
                        connection._writer.WriteLine($"{DateTime.UtcNow} - Another hour has passed.");
                        connection._writer.Flush();
                    }
                }
                Console.WriteLine(minute);
                Thread.Sleep(Constants.PulseTimer);
            }
        }
    }
}
