using MTD.Haven.Domain;
using MTD.Haven.Domain.Models.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MTD.Haven.Dals.Implementation
{
    public class PlayerDal : IPlayerDal
    {
        public List<Player> GetOnlinePlayers()
        {
            return GetAllPlayers().Where(p => p.IsOnline).ToList();
        }

        public Player GetPlayerByName(string name)
        {
            var player = File.ReadAllText($"{Constants.PlayerDirectory}{name}.json");

            return JsonConvert.DeserializeObject<Player>(player);
        }

        public List<Player> GetAllPlayers()
        {
            var players = new List<Player>();

            foreach(var p in GetPlayerFiles())
            {
                players.Add(JsonConvert.DeserializeObject<Player>(File.ReadAllText(p)));
            }

            return players;
        }

        private List<string> GetPlayerFiles()
        {
            return Directory.GetFiles(Constants.PlayerDirectory).ToList();
        }
    }
}
