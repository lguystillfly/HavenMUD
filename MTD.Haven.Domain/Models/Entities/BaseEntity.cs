using System;

namespace MTD.Haven.Domain.Models.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int CurrentRoom { get; set; }

        void Move(int id)
        {
            CurrentRoom = id;
        }
    }
}
