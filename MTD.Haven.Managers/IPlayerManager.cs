using MTD.Haven.Domain.Models.Entities;
using System.Collections.Generic;

namespace MTD.Haven.Managers
{
    public interface IPlayerManager
    {
        Player GetPlayerByName(string name);
        List<Player> GetOnlinePlayers();
        List<Player> GetAllPlayers();
    }
}
