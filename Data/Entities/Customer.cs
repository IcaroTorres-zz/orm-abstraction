using System;
using System.Collections.Generic;

namespace Data.Entities
{
    public class Customer 
    {
        public Guid Id { get; set; } = System.Guid.NewGuid();
        public string Name { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
