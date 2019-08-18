using System;
using System.Collections.Generic;

namespace CrossORM.Entities
{
    public class Customer : Entity<Guid>
    {
        public string Name { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
