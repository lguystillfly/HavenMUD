using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTD.Haven.Domain;
using MTD.Haven.Domain.Models.Entities;
using MTD.Haven.Domain.Models.World;
using Newtonsoft.Json;

namespace MTD.Haven.Dals.Implementation
{
    public class RoomDal : IRoomDal
    {
        public List<Room> GetRooms()
        {
            var regions = new List<Room>();

            foreach (var p in GetRoomFiles())
            {
                regions.Add(JsonConvert.DeserializeObject<Room>(File.ReadAllText(p)));
            }

            return regions;
        }

        private List<string> GetRoomFiles()
        {
            return Directory.GetFiles(Constants.RoomDirectory).ToList();
        }
    }
}