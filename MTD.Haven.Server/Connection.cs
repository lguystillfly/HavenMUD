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
    public class Connection : IDisposable
    {
        static object _bigLock = new object();
        private readonly Socket _socket;
        private readonly StreamReader _reader;
        public readonly StreamWriter _writer;
        public readonly List<Connection> _connections = new List<Connection>();
        private string _playerLogin;
        private Player _player;

        private readonly IPlayerManager _playerManager;

        public Connection(Socket socket, IPlayerManager playerManager)
        {
            _socket = socket;
            _reader = new StreamReader(new NetworkStream(socket, false));
            _writer = new StreamWriter(new NetworkStream(socket, true));
            new Thread(ClientLoop).Start();
            _playerLogin = "";
            _player = new Player();
            _playerManager = playerManager;
        }

        void ClientLoop()
        {
            try
            {
                lock (_bigLock)
                {
                    OnConnect();
                }
                while (true)
                {
                    lock (_bigLock)
                    {
                        foreach (Connection conn in _connections)
                        {
                            conn._writer.Flush();
                        }
                    }

                    string line = _reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    lock (_bigLock)
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
                lock (_bigLock)
                {
                    if (_socket != null)
                    {
                        _socket.Close();
                    }

                    OnDisconnect();
                }
            }
        }

        void OnConnect()
        {
            _writer.WriteLine("Please enter your name: ");
            _writer.Flush();
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
                _player.Id = Guid.NewGuid();
                _player.Name = _playerLogin;
                _player.Title = "is here.";
                _player.CreatedDate = DateTime.UtcNow;
                _player.ModifiedDate = DateTime.UtcNow;
                _player.LastLogin = DateTime.UtcNow;
                _player.CurrentRoom = 1;
                _player.IsOnline = true;
            }

            SavePlayer(_player);

            _writer.WriteLine($"Welcome, {_player.Name}!");

            DisplayRoom(_player.CurrentRoom);

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

            _writer.WriteLine(builder.ToString());
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

                _writer.WriteLine(builder.ToString());
            }
            else if(line.ToLower().StartsWith("say "))
            {
                foreach (Connection conn in _connections)
                {
                    if (conn._player.CurrentRoom == _player.CurrentRoom)
                    {
                        conn._writer.WriteLine($"{_player.Name} says, '{line.Replace("say ", "").Trim()}'");
                    }
                }
            }
            else if(line.ToLower().StartsWith("title "))
            {
                var title = line.Replace("title ", "");

                _player.Title = title;
                File.WriteAllText($"{Constants.PlayerDirectory}{_playerLogin}.json", JsonConvert.SerializeObject(_player));

                _writer.WriteLine("Your new title has been set.");
            }
            else if(line.ToLower().Equals("look"))
            {
                DisplayRoom(_player.CurrentRoom);
            }
            else
            {
                _writer.WriteLine("Unknown Command.");
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
            _socket?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}