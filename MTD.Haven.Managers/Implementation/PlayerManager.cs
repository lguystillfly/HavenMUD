using MTD.Haven.Dals;
using MTD.Haven.Domain.Models.Entities;
using System;
using System.Collections.Generic;

namespace MTD.Haven.Managers.Implementation
{
    public class PlayerManager : IPlayerManager
    {
        private readonly IPlayerDal _playerDal;

        public PlayerManager(IPlayerDal playerDal)
        {
            _playerDal = playerDal;
        }

        public List<Player> GetAllPlayers()
        {
            return _playerDal.GetAllPlayers();
        }

        public List<Player> GetOnlinePlayers()
        {
            return _playerDal.GetOnlinePlayers();
        }

        public Player GetPlayerByName(string name)
        {
            return _playerDal.GetPlayerByName(name);
        }
    }
}
