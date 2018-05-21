using MTD.Haven.Domain.Models.Entities;
using System.Collections.Generic;

namespace MTD.Haven.Dals
{
    public interface IPlayerDal
    {
        Player GetPlayerByName(string name);
        List<Player> GetOnlinePlayers();
        List<Player> GetAllPlayers();
    }
}
