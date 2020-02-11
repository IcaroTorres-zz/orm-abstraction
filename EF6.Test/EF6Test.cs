using Data.Abstractions;
using Data.Concrete.EF6;
using Data.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Linq;

namespace EF6Test
{
    [TestClass]
    public class EF6Test
    {
        private readonly IDataSource _source;

        public EF6Test()
        {
            var connection = Effort.DbConnectionFactory.CreateTransient();

            _source = new EF6Source(new EF6Context(connection));
        }

        [TestMethod]
        public void EF6SourceTest()
        {
            using (var dataSource = _source.BeginTransaction())
            {
                // arrange
                var customer = new Customer { Name = "dummy" };
                var product1 = new Product("dummy product1", 35)
                {
                    AmountValue = 29.30M,
                    Description = "A dummy product 1 description"
                };
                var product2 = new Product("dummy product2", 13)
                {
                    AmountValue = 109.90M,
                    Description = "A dummy product 2 description"
                };
                var orderProduct1 = new OrderProduct(product1, 15);
                var orderProduct2 = new OrderProduct(product2, 5);
                var order = new Order(customer, new OrderProduct[] { orderProduct1, orderProduct2 });

                // act
                _source.Set<Customer>().Add(customer);
                _source.Set<Product>().Add(product1);
                _source.Set<Product>().Add(product2);
                _source.Set<OrderProduct>().Add(orderProduct1);
                _source.Set<OrderProduct>().Add(orderProduct2);
                _source.Set<Order>().Add(order);

                _source.CommitState();

                var returnOrder = _source.Set<Order>()
                    .Where(o => o.CustomerId == customer.Id)
                    .Include(o => o.Customer)
                    .Include(o => o.OrderProducts)
                    .AsNoTracking()
                    .Single();

                // assert
                Assert.AreEqual(order.Id, returnOrder.Id);
                Assert.AreEqual(order.TotalAmount, returnOrder.TotalAmount);
                Assert.AreEqual(order.Customer.Name, returnOrder.Customer.Name);
                Assert.AreEqual(order.OrderProducts.Count, returnOrder.OrderProducts.Count);

                dataSource.CommitTransaction();
            }
        }
    }
}
