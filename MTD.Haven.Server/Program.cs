using Microsoft.Extensions.DependencyInjection;
using MTD.Haven.Dals;
using MTD.Haven.Dals.Implementation;
using MTD.Haven.Managers;
using MTD.Haven.Managers.Implementation;
using System.Net;
using System.Net.Sockets;

namespace MTD.Haven.Server
{
    class Program
    {
        const int PortNumber = 4000;
        const int BacklogSize = 20;

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
            while (true)
            {
                Socket conn = server.Accept();
                new Connection(conn, serviceProvider.GetRequiredService<IPlayerManager>());
            }
        }
    }
}
