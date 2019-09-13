using System;

namespace CrossDomain.Entities
{
    public class Entity<TKey>
    {
        public TKey Id { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
