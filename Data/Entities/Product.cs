using System;
using System.Collections.Generic;

namespace Data.Entities
{
    public class Product
    {
        protected Product() { }
        public Guid Id { get; set; } = System.Guid.NewGuid();
        public Product(string name, int count)
        {
            UpdateName(name);
            UpdateStoreCountTo(count);
        }
        public string Name { get; private set; }
        public decimal AmountValue { get; set; }
        public string Description { get; set; }
        public int StockCount { get; private set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }

        public Product UpdateName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentNullException($"{nameof(Name)} cannot be null or empty string", newName);

            Name = newName;
            return this;
        }
        public Product UpdateStoreCountTo(int count)
        {
            if ((StockCount + count) < 0)
                throw new ArgumentException("Given count is bigger than available");

            StockCount += Math.Abs(count);
            return this;
        }
    }
}
