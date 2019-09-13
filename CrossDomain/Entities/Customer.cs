using System;
using System.Collections.Generic;

namespace CrossDomain.Entities
{
    public class Customer : Entity<Guid>
    {
        public Customer() { }
        public new Guid Id { get; set; } = System.Guid.NewGuid();
        public string Name { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
