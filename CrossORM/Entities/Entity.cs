using System;

namespace CrossORM.Entities
{
    public abstract class Entity<TKey>
    {
        public TKey Id { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
