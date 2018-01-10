using MTD.Haven.Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTD.Haven.Dals
{
    public interface IPlayerDal
    {
        Player GetPlayerByName(string name);
        List<Player> GetOnlinePlayers();
        List<Player> GetAllPlayers();
    }
}
