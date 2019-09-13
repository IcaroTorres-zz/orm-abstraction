using System;
using System.Collections.Generic;

namespace CrossDomain.Entities
{
    public class Customer : Entity<Guid>
    {
        public Customer() { }
        public string Name { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
