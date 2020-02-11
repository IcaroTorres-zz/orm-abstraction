using System;

namespace Data.Entities
{
    public class Entity
    {
        public bool IsDisabled { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
