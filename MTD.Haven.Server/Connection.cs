using MTD.Haven.Domain;
using MTD.Haven.Domain.Models.Entities;
using MTD.Haven.Domain.Models.World;
using MTD.Haven.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MTD.Haven.Server
{
    public class Connection
    {
        static object BigLock = new object();
        Socket socket;
        public StreamReader Reader;
        public StreamWriter Writer;
        static List<Connection> connections = new List<Connection>();
        public string playerLogin;
        public Player Player;

        private readonly IPlayerManager _playerManager;

        public Connection(Socket socket, IPlayerManager playerManager)
        {
            socket = socket;
            Reader = new StreamReader(new NetworkStream(socket, false));
            Writer = new StreamWriter(new NetworkStream(socket, true));
            new Thread(ClientLoop).Start();
            new Thread(ServerLoop).Start();
            playerLogin = "";
            Player = new Player();
            _playerManager = playerManager;
        }

        void ServerLoop()
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

                    foreach(var connection in connections)
                    {
                        connection.Writer.WriteLine($"{DateTime.UtcNow} - Another minute has passed.");
                        connection.Writer.Flush();
                    }
                }

                if (half >= Constants.Half)
                {
                    half = 0;

                    foreach (var connection in connections)
                    {
                        connection.Writer.WriteLine($"{DateTime.UtcNow} - Another half hour has passed.");
                        connection.Writer.Flush();
                    }
                }

                if (hour >= Constants.Hour)
                {
                    hour = 0;

                    foreach (var connection in connections)
                    {
                        connection.Writer.WriteLine($"{DateTime.UtcNow} - Another hour has passed.");
                        connection.Writer.Flush();
                    }
                }
                Console.WriteLine(minute);
                Thread.Sleep(Constants.PulseTimer);
            }
        }

        void ClientLoop()
        {
            try
            {
                lock (BigLock)
                {
                    OnConnect();
                }
                while (true)
                {
                    lock (BigLock)
                    {
                        foreach (Connection conn in connections)
                        {
                            conn.Writer.Flush();
                        }
                    }

                    string line = Reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    lock (BigLock)
                    {
                        ProcessLine(line);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in ClientLoop: {ex.Message} - {ex.StackTrace}");
            }
            finally
            {
                lock (BigLock)
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }

                    OnDisconnect();
                }
            }
        }

        void OnConnect()
        {
            Writer.WriteLine("Please enter your name: ");
            Writer.Flush();
            playerLogin = Reader.ReadLine();

            try
            {
                Player = _playerManager.GetPlayerByName(playerLogin);

                if (Player == null)
                {
                    throw new Exception();
                }

                Player.IsOnline = true;
                Player.LastLogin = DateTime.UtcNow;
            }
            catch(Exception)
            {
                Player.Id = Guid.NewGuid();
                Player.Name = playerLogin;
                Player.Title = "is here.";
                Player.CreatedDate = DateTime.UtcNow;
                Player.ModifiedDate = DateTime.UtcNow;
                Player.LastLogin = DateTime.UtcNow;
                Player.CurrentRoom = 1;
                Player.IsOnline = true;
            }

            SavePlayer(Player);

            Writer.WriteLine($"Welcome, {Player.Name}!");

            DisplayRoom(Player.CurrentRoom);

            connections.Add(this);
        }

        public void DisplayRoom(int id)
        {
            var room = GetRoomById(id);
            var builder = new StringBuilder();

            builder.AppendLine(room.Title);
            builder.AppendLine("-");
            builder.AppendLine(room.Description);
            builder.AppendLine("-");
            builder.AppendLine("Known Exits: ");

            if (room.Exits.Count > 0)
            {
                foreach (var e in room.Exits)
                {
                    builder.AppendLine("" + e.Direction);
                }
            }
            else
            {
                builder.AppendLine("None");
            }

            builder.AppendLine("Also here: ");

            bool containsOthers = false;
            foreach(var player in GetOnlinePlayersInRoom(Player.CurrentRoom).Where(p => p.Id != Player.Id))
            {
                builder.AppendLine(player.Name);
                containsOthers = true;
            }

            if(!containsOthers)
            {
                builder.AppendLine("No one");
            }

            Writer.WriteLine(builder.ToString());
        }

        void OnDisconnect()
        {
            Player.IsOnline = false;
            SavePlayer(Player);
            connections.Remove(this);
        }

        void ProcessLine(string line)
        {
            if (line.ToLower().Equals("who"))
            {
                var builder = new StringBuilder();
                builder.AppendLine("-=-=-=-=-=-=-=-=-=[ Haven Users ]=-=-=-=-=-=-=-=-=-");
                foreach (var connection in connections)
                {
                    builder.AppendLine($"{connection.Player.Name} - {connection.Player.Title}");
                }
                builder.AppendLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
                builder.AppendLine($"{connections.Count} players online.");

                Writer.WriteLine(builder.ToString());
            }
            else if(line.ToLower().StartsWith("say "))
            {
                foreach (Connection conn in connections)
                {
                    conn.Writer.WriteLine($"{Player.Name} says, '{line.Replace("say ", "").Trim()}'");
                }
            }
            else if(line.ToLower().StartsWith("title "))
            {
                var title = line.Replace("title ", "");

                Player.Title = title;
                File.WriteAllText($"{Constants.PlayerDirectory}{playerLogin}.json", JsonConvert.SerializeObject(Player));

                Writer.WriteLine("Your new title has been set.");
            }
            else if(line.ToLower().Equals("look"))
            {
                DisplayRoom(Player.CurrentRoom);
            }
            else
            {
                Writer.WriteLine("Unknown Command.");
            }
        }



        public Player SavePlayer(Player player)
        {
            File.WriteAllText($"{Constants.PlayerDirectory}{player.Name}.json", JsonConvert.SerializeObject(player));

            return player;
        }

        public List<Player> GetOnlinePlayersInRoom(int id)
        {
            var onlinePlayerList = new List<Player>();
            var playerFileList = Directory.GetFiles(Constants.PlayerDirectory);

            foreach(var pf in playerFileList)
            {
                var player = JsonConvert.DeserializeObject<Player>(File.ReadAllText(pf));

                if(player.IsOnline && player.CurrentRoom == id)
                {
                    onlinePlayerList.Add(player);
                }
            }

            return onlinePlayerList;
        }

        public Room GetRoomById(int id)
        {
            return JsonConvert.DeserializeObject<Room>(File.ReadAllText($"{Constants.RoomDirectory}{id}.json"));
        }
    }
}