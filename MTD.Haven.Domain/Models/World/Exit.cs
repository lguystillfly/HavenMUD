using MTD.Haven.Domain.Enumerations;
using System.Collections.Generic;

namespace MTD.Haven.Domain.Models.World
{
    public class Exit
    {
        public int Id { get; set; }
        public int RoomFrom { get; set; }
        public int RoomTo { get; set; }
        public bool IsLocked { get; set; }
        public int? KeyId { get; set; }
        public CompassDirection Direction { get; set; }
        public List<string> Aliases { get; set; }
    }
}
