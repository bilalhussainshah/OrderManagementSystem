using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrderManagementSystem.Controllers;
using OrderManagementSystem.Models;
using OrderManagementSystem.Models.DTOs;
using OrderManagementSystem.Tests.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace OrderManagementSystem.Tests.Tests
{
    [TestClass]
    public class OrdersControllerTests
    {
        private Mock<IOrderManagementDBEntities> _mockDb;
        private OrdersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockDb = new Mock<IOrderManagementDBEntities>();
            _controller = new OrdersController(_mockDb.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        // GET ALL ORDERS

        [TestMethod]
        public async Task GetOrders_ReturnsAllOrders()
        {
            var customer = new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" };
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    CustomerId = 1,
                    Customer = customer,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { Quantity = 2, UnitPrice = 10m }
                    }
                }
            };
            _mockDb.Setup(db => db.Orders).Returns(new TestDbSet<Order>(orders));

            var result = await _controller.GetOrders() as OkNegotiatedContentResult<List<OrderDto>>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Content.Count);
            Assert.AreEqual("Alice", result.Content[0].CustomerName);
            Assert.AreEqual(20m, result.Content[0].Total);
        }

        // GET ORDER BY ID

        [TestMethod]
        public async Task GetOrder_ExistingId_ReturnsOrderDetail()
        {
            var product = new Product { Id = 1, Name = "Chair", Price = 50m, StockQuantity = 10 };
            var customer = new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" };
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    CustomerId = 1,
                    Customer = customer,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { Id = 1, ProductId = 1, Product = product, Quantity = 1, UnitPrice = 50m }
                    }
                }
            };
            _mockDb.Setup(db => db.Orders).Returns(new TestDbSet<Order>(orders));

            var result = await _controller.GetOrder(1) as OkNegotiatedContentResult<OrderDetailDto>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Content.Id);
            Assert.AreEqual("Alice", result.Content.CustomerName);
            Assert.AreEqual(1, result.Content.Items.Count);
            Assert.AreEqual("Chair", result.Content.Items[0].ProductName);
        }

        [TestMethod]
        public async Task GetOrder_NonExistingId_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Orders).Returns(new TestDbSet<Order>());

            var result = await _controller.GetOrder(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // POST (CREATE) ORDER

        [TestMethod]
        public async Task CreateOrder_NullModel_ReturnsBadRequest()
        {
            var result = await _controller.CreateOrder(null);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task CreateOrder_NoProducts_ReturnsBadRequest()
        {
            var model = new OrderViewModel
            {
                CustomerId = 1,
                Products = new List<ProductSelection>()
            };

            var result = await _controller.CreateOrder(model);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task CreateOrder_InvalidCustomer_ReturnsBadRequest()
        {
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>());

            var model = new OrderViewModel
            {
                CustomerId = 999,
                Products = new List<ProductSelection>
                {
                    new ProductSelection { ProductId = 1, Quantity = 1 }
                }
            };

            var result = await _controller.CreateOrder(model);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }


        [TestMethod]
        public async Task CreateOrder_InsufficientStock_ReturnsBadRequest()
        {
            var customers = new List<Customer>
    {
        new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" }
    };
            var products = new List<Product>
    {
        new Product { Id = 1, Name = "Chair", Price = 50m, StockQuantity = 1 }
    };

            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>(customers));
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>(products));
            _mockDb.Setup(db => db.Orders).Returns(new TestDbSet<Order>());
            _mockDb.Setup(db => db.OrderItems).Returns(new TestDbSet<OrderItem>());
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var model = new OrderViewModel
            {
                CustomerId = 1,
                Products = new List<ProductSelection>
        {
            new ProductSelection { ProductId = 1, Quantity = 99 }
        }
            };

            var result = await _controller.CreateOrder(model);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

    }
}