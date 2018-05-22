using System.Collections.Generic;

namespace MTD.Haven.Domain.Models.World
{
    public class Region
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Area> Areas { get; set; }
    }
}
