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
    public class CustomersControllerTests
    {
        private Mock<IOrderManagementDBEntities> _mockDb;
        private CustomersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockDb = new Mock<IOrderManagementDBEntities>();

            _controller = new CustomersController(_mockDb.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        // =============================================
        // GET ALL CUSTOMERS
        // =============================================

        [TestMethod]
        public async Task GetCustomers_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" },
                new Customer { Id = 2, Name = "Bob",   Email = "bob@test.com" }
            };
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>(customers));

            // Act
            var result = await _controller.GetCustomers() as OkNegotiatedContentResult<List<CustomerDto>>;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Content.Count);
        }

        [TestMethod]
        public async Task GetCustomers_EmptyList_ReturnsEmpty()
        {
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>());

            var result = await _controller.GetCustomers() as OkNegotiatedContentResult<List<CustomerDto>>;

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Content.Count);
        }

        // =============================================
        // GET CUSTOMER BY ID
        // =============================================

        [TestMethod]
        public async Task GetCustomer_ExistingId_ReturnsCustomer()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" }
            };
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>(customers));

            var result = await _controller.GetCustomer(1) as OkNegotiatedContentResult<CustomerDto>;

            Assert.IsNotNull(result);
            Assert.AreEqual("Alice", result.Content.Name);
            Assert.AreEqual("alice@test.com", result.Content.Email);
        }

        [TestMethod]
        public async Task GetCustomer_NonExistingId_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>());

            var result = await _controller.GetCustomer(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // =============================================
        // POST (CREATE) CUSTOMER
        // =============================================

        [TestMethod]
        public async Task PostCustomer_ValidData_ReturnsOk()
        {
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>());
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new CustomerDto { Name = "Charlie", Email = "charlie@test.com" };

            var result = await _controller.PostCustomer(dto) as OkNegotiatedContentResult<CustomerDto>;

            Assert.IsNotNull(result);
            Assert.AreEqual("charlie@test.com", result.Content.Email);
        }

        [TestMethod]
        public async Task PostCustomer_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.PostCustomer(null);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task PostCustomer_DuplicateEmail_ReturnsBadRequest()
        {
            var existing = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" }
            };
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>(existing));

            var dto = new CustomerDto { Name = "Alice2", Email = "alice@test.com" };

            var result = await _controller.PostCustomer(dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        // =============================================
        // PUT (UPDATE) CUSTOMER
        // =============================================

        [TestMethod]
        public async Task PutCustomer_ValidUpdate_ReturnsOk()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" }
            };
            var dbSet = new TestDbSet<Customer>(customers);
            _mockDb.Setup(db => db.Customers).Returns(dbSet);
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new CustomerDto { Id = 1, Name = "Alice Updated", Email = "alice_new@test.com" };

            var result = await _controller.PutCustomer(1, dto);

            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<string>));
        }

        [TestMethod]
        public async Task PutCustomer_IdMismatch_ReturnsBadRequest()
        {
            var dto = new CustomerDto { Id = 5, Name = "X", Email = "x@x.com" };

            var result = await _controller.PutCustomer(1, dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task PutCustomer_NonExistingId_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>());

            var dto = new CustomerDto { Id = 999, Name = "X", Email = "x@x.com" };

            var result = await _controller.PutCustomer(999, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // =============================================
        // DELETE CUSTOMER
        // =============================================

        [TestMethod]
        public async Task DeleteCustomer_ExistingWithNoOrders_ReturnsOk()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" }
            };
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>(customers));
            _mockDb.Setup(db => db.Orders).Returns(new TestDbSet<Order>());
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _controller.DeleteCustomer(1);

            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<string>));
        }

        [TestMethod]
        public async Task DeleteCustomer_WithExistingOrders_ReturnsBadRequest()
        {
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice", Email = "alice@test.com" }
            };
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 1 }
            };
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>(customers));
            _mockDb.Setup(db => db.Orders).Returns(new TestDbSet<Order>(orders));

            var result = await _controller.DeleteCustomer(1);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task DeleteCustomer_NonExisting_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Customers).Returns(new TestDbSet<Customer>());

            var result = await _controller.DeleteCustomer(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}