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
using MTD.Haven.Domain.Enumerations;

namespace MTD.Haven.Server
{
    public class Connection : IDisposable
    {
        private static readonly object BigLock = new object();
        private readonly TcpClient _client;
        private readonly StreamReader _reader;
        public readonly StreamWriter Writer;
        private string _playerLogin;
        private Player _player;

        private readonly IPlayerManager _playerManager;

        private readonly List<Connection> _connections;

        public Connection(TcpClient client, IPlayerManager playerManager, List<Connection> connections)
        {
            _client = client;
            Stream stream = _client.GetStream();
            Writer = new StreamWriter(stream);
            _reader = new StreamReader(Writer.BaseStream);
            new Thread(ClientLoop).Start();
            _playerLogin = "";
            _player = new Player();
            _playerManager = playerManager;
            _connections = connections;
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
                        foreach (Connection conn in _connections)
                        {
                            conn.Writer.Flush();
                        }
                    }

                    string line = _reader.ReadLine();
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
                    _client?.Close();

                    OnDisconnect();
                }
            }
        }

        void OnConnect()
        {
            Writer.WriteLine("Please enter your name: ");
            Writer.Flush();
            _playerLogin = _reader.ReadLine();

            try
            {
                _player = _playerManager.GetPlayerByName(_playerLogin);

                if (_player == null)
                {
                    throw new Exception("Player is null. Why :(");
                }

                _player.IsOnline = true;
                _player.LastLogin = DateTime.UtcNow;
            }
            catch(Exception)
            {
                if (_player != null)
                {
                    _player.Id = Guid.NewGuid();
                    _player.Name = _playerLogin;
                    _player.Title = "is here.";
                    _player.CreatedDate = DateTime.UtcNow;
                    _player.ModifiedDate = DateTime.UtcNow;
                    _player.LastLogin = DateTime.UtcNow;
                    _player.CurrentRoom = 1;
                    _player.IsOnline = true;
                }
            }

            SavePlayer(_player);

            Writer.WriteLine($"Welcome, {_player.Name}!");

            DisplayRoom(_player.CurrentRoom);

            foreach (Connection conn in _connections)
            {
                conn.Writer.WriteLine($"{_player.Name} has entered the Realm.");
            }

            _connections.Add(this);
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
            foreach(var player in GetOnlinePlayersInRoom(_player.CurrentRoom).Where(p => p.Id != _player.Id))
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
            _player.IsOnline = false;
            SavePlayer(_player);
            _connections.Remove(this);
        }

        void ProcessLine(string line)
        {
            if (line.ToLower().Equals("who"))
            {
                var builder = new StringBuilder();
                builder.AppendLine("-=-=-=-=-=-=-=-=-=[ Haven Users ]=-=-=-=-=-=-=-=-=-");
                foreach (var connection in _connections)
                {
                    builder.AppendLine($"{connection._player.Name} - {connection._player.Title}");
                }
                builder.AppendLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
                builder.AppendLine($"{_connections.Count} players online.");

                Writer.WriteLine(builder.ToString());
            }
            else if(line.ToLower().StartsWith("say "))
            {
                foreach (Connection conn in _connections)
                {
                    if (conn._player.CurrentRoom == _player.CurrentRoom)
                    {
                        conn.Writer.WriteLine($"{_player.Name} says, '{line.Replace("say ", "").Trim()}'");
                    }
                }
            }
            else if(line.ToLower().StartsWith("title "))
            {
                var title = line.Replace("title ", "");

                _player.Title = title;
                File.WriteAllText($"{Constants.PlayerDirectory}{_playerLogin}.json", JsonConvert.SerializeObject(_player));

                Writer.WriteLine("Your new title has been set.");
            }
            else if(line.ToLower().Equals("look"))
            {
                DisplayRoom(_player.CurrentRoom);
            }
            else if (line.ToLower().Equals("north") || line.ToLower().Equals("n") ||
                     line.ToLower().Equals("east") || line.ToLower().Equals("e") ||
                     line.ToLower().Equals("south") || line.ToLower().Equals("s") ||
                     line.ToLower().Equals("west") || line.ToLower().Equals("w"))
            {

                var room = GetRoomById(_player.CurrentRoom);

                var direction = CompassDirection.North;

                switch (line.ToLower())
                {
                    case "south":
                    case "s":
                        direction = CompassDirection.South;
                        break;
                    case "west":
                    case "w":
                        direction = CompassDirection.West;
                        break;
                    case "east":
                    case "e":
                        direction = CompassDirection.East;
                        break;
                }

                var newRoom = room.Exits.FirstOrDefault(e => e.Direction == direction);

                if (newRoom == null)
                {
                    Writer.WriteLine("There is no exist that way.");
                }
                else
                {
                    Writer.WriteLine("There is an exit that way.");
                    _player.Move(newRoom.RoomTo);
                    DisplayRoom(_player.CurrentRoom);
                }
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

        public void Dispose()
        {
            _client?.Dispose();
            _reader?.Dispose();
            Writer?.Dispose();
        }
    }
}