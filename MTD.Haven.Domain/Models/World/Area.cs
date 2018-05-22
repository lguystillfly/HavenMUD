using System.Collections.Generic;

namespace MTD.Haven.Domain.Models.World
{
    public class Area
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Room> Rooms { get; set; }
    }
}
