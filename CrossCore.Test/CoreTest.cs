using CrossCore;
using CrossDomain;
using CrossDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CoreTest
{
    [TestClass]
    public class CoreTest
    {
        private readonly IService _service;

        public CoreTest()
        {
            var options = new DbContextOptionsBuilder<ContextCore>()
                .UseInMemoryDatabase(databaseName: DateTime.UtcNow.Ticks.ToString())
                .Options;

            _service = new ServiceCore(new ContextCore(options));
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GenericServiceTest()
        {
            using (_service)
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
                var orderProduct1 = new OrderProduct(ref product1, 15);
                var orderProduct2 = new OrderProduct(ref product2, 5);
                var order = new Order(customer, new OrderProduct[] { orderProduct1, orderProduct2 });

                // act
                _service.Add<Customer>(customer);
                _service.Add<Product>(product1);
                _service.Add<Product>(product2);
                _service.Add<OrderProduct>(orderProduct1);
                _service.Add<OrderProduct>(orderProduct2);
                _service.Add<Order>(order);

                _service.Commit(false);

                var returnOrder = await _service.Find<Order>(o => o.CustomerId == customer.Id,
                                                             includes: "Customer,OrderProducts",
                                                             isreadonly: true).SingleAsync();
                // assert
                Assert.AreEqual(order.Id, returnOrder.Id);
                Assert.AreEqual(order.TotalAmount, returnOrder.TotalAmount);
                Assert.AreEqual(order.Customer.Name, returnOrder.Customer.Name);
                Assert.AreEqual(order.OrderProducts.Count, returnOrder.OrderProducts.Count);
            }
        }
    }
}
