using System;
using System.Collections.Generic;
using System.Text;

namespace MTD.Haven.Domain.Models.Entities
{
    public class Player : BaseEntity
    {
        public bool IsOnline { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
