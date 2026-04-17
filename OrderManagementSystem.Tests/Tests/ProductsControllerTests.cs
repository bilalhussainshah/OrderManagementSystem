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
    public class ProductsControllerTests
    {
        private Mock<IOrderManagementDBEntities> _mockDb;
        private ProductsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockDb = new Mock<IOrderManagementDBEntities>();
            _controller = new ProductsController(_mockDb.Object);
            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
        }

        // =============================================
        // GET ALL PRODUCTS
        // =============================================

        [TestMethod]
        public async Task GetProducts_ReturnsAllProducts()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Chair", Price = 49.99m, StockQuantity = 10 },
                new Product { Id = 2, Name = "Desk",  Price = 199.99m, StockQuantity = 5 }
            };
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>(products));

            var result = await _controller.GetProducts() as OkNegotiatedContentResult<List<ProductDto>>;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Content.Count);
        }

        // =============================================
        // GET PRODUCT BY ID
        // =============================================

        [TestMethod]
        public async Task GetProduct_ExistingId_ReturnsProduct()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Chair", Price = 49.99m, StockQuantity = 10 }
            };
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>(products));

            var result = await _controller.GetProduct(1) as OkNegotiatedContentResult<ProductDto>;

            Assert.IsNotNull(result);
            Assert.AreEqual("Chair", result.Content.Name);
            Assert.AreEqual(49.99m, result.Content.Price);
        }

        [TestMethod]
        public async Task GetProduct_NonExistingId_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>());

            var result = await _controller.GetProduct(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // =============================================
        // POST (CREATE) PRODUCT
        // =============================================

        [TestMethod]
        public async Task PostProduct_ValidData_ReturnsOk()
        {
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>());
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new ProductDto { Name = "Lamp", Price = 29.99m, StockQuantity = 20 };

            var result = await _controller.PostProduct(dto) as OkNegotiatedContentResult<ProductDto>;

            Assert.IsNotNull(result);
            Assert.AreEqual("Lamp", result.Content.Name);
        }

        [TestMethod]
        public async Task PostProduct_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.PostProduct(null);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task PostProduct_NegativePrice_ReturnsBadRequest()
        {
            var dto = new ProductDto { Name = "Bad", Price = -5m, StockQuantity = 10 };

            var result = await _controller.PostProduct(dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task PostProduct_NegativeStock_ReturnsBadRequest()
        {
            var dto = new ProductDto { Name = "Bad", Price = 10m, StockQuantity = -1 };

            var result = await _controller.PostProduct(dto);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        // =============================================
        // PUT (UPDATE) PRODUCT
        // =============================================

        [TestMethod]
        public async Task PutProduct_ValidUpdate_ReturnsOk()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Chair", Price = 49.99m, StockQuantity = 10 }
            };
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>(products));
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var dto = new ProductDto { Id = 1, Name = "Updated Chair", Price = 59.99m, StockQuantity = 15 };

            var result = await _controller.PutProduct(1, dto);

            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<string>));
        }

        [TestMethod]
        public async Task PutProduct_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.PutProduct(1, null);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task PutProduct_NonExistingId_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>());

            var dto = new ProductDto { Id = 999, Name = "X", Price = 10m, StockQuantity = 1 };

            var result = await _controller.PutProduct(999, dto);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // =============================================
        // DELETE PRODUCT
        // =============================================

        [TestMethod]
        public async Task DeleteProduct_ExistingNotInOrders_ReturnsOk()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Chair", Price = 49.99m, StockQuantity = 10 }
            };
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>(products));
            _mockDb.Setup(db => db.OrderItems).Returns(new TestDbSet<OrderItem>());
            _mockDb.Setup(db => db.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _controller.DeleteProduct(1);

            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<string>));
        }

        [TestMethod]
        public async Task DeleteProduct_UsedInOrders_ReturnsBadRequest()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Chair", Price = 49.99m, StockQuantity = 10 }
            };
            var orderItems = new List<OrderItem>
            {
                new OrderItem { Id = 1, ProductId = 1, OrderId = 1, Quantity = 2, UnitPrice = 49.99m }
            };
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>(products));
            _mockDb.Setup(db => db.OrderItems).Returns(new TestDbSet<OrderItem>(orderItems));

            var result = await _controller.DeleteProduct(1);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task DeleteProduct_NonExisting_ReturnsNotFound()
        {
            _mockDb.Setup(db => db.Products).Returns(new TestDbSet<Product>());

            var result = await _controller.DeleteProduct(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}