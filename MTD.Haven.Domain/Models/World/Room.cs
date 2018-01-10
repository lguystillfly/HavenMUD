using System.Collections.Generic;

namespace MTD.Haven.Domain.Models.World
{
    public class Room
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Exit> Exits { get; set; }
    }
}
