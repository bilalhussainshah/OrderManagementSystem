using OrderManagementSystem.Models;
using OrderManagementSystem.Models.DTOs;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace OrderManagementSystem.Controllers
{
    public class OrdersController : ApiController
    {
        private readonly IOrderManagementDBEntities db;

        public OrdersController()
        {
            db = new OrderManagementDBEntities();
        }

        public OrdersController(IOrderManagementDBEntities db)
        {
            this.db = db;
        }
        //private readonly OrderManagementDBEntities db = new OrderManagementDBEntities();

        // GET ALL ORDERS
        [HttpGet]
        public async Task<IHttpActionResult> GetOrders()
        {
            try
            {
                Log.Information("Fetching all orders");

                var orders = await db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                    .Select(o => new OrderDto
                    {
                        Id = o.Id,
                        OrderDate = o.OrderDate,
                        CustomerName = o.Customer.Name,
                        Total = o.OrderItems.Sum(i => i.Quantity * i.UnitPrice)
                    })
                    .ToListAsync();

                Log.Information("Fetched {Count} orders", orders.Count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching orders");
                return InternalServerError();
            }
        }

        // GET ORDER BY ID
        [HttpGet]
        public async Task<IHttpActionResult> GetOrder(int id)
        {
            try
            {
                Log.Information("Fetching order {OrderId}", id);
                var order = await db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems.Select(i => i.Product))
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    Log.Warning("Order not found: {OrderId}", id);
                    return NotFound();
                }

                var dto = new OrderDetailDto
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    CustomerName = order.Customer.Name,
                    Items = order.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                Log.Information("Order {OrderId} fetched successfully", id);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching order {OrderId}", id);
                return InternalServerError();
            }
        }

        // CREATE ORDER (PRODUCTION-GRADE)
        [HttpPost]
        public async Task<IHttpActionResult> CreateOrder(OrderViewModel model)
        {
            //  BASIC VALIDATION
            if (model == null)
            {
                Log.Warning("CreateOrder failed: model is null");
                return BadRequest("Invalid request");
            }

            if (model.Products == null || !model.Products.Any())
            {
                Log.Warning("CreateOrder failed: no products selected");
                return BadRequest("Select at least one product");
            }
            try
            {
                //  FETCH DATA FIRST
                var customer = await db.Customers
                    .FirstOrDefaultAsync(c => c.Id == model.CustomerId);

                if (customer == null)
                {
                    Log.Warning("CreateOrder failed: invalid customer {CustomerId}", model.CustomerId);
                    return BadRequest("Invalid customer");
                }
                var productIds = model.Products.Select(p => p.ProductId).ToList();

                var products = await db.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                //  VALIDATE ALL ITEMS
                foreach (var item in model.Products)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                    if (product == null)
                    {
                        Log.Warning("Product not found: {ProductId}", item.ProductId);
                        return BadRequest($"Product not found: {item.ProductId}");
                    }

                    if (item.Quantity <= 0)
                    {
                        Log.Warning("Invalid quantity for ProductId: {ProductId}", item.ProductId);
                        return BadRequest("Quantity must be at least 1");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        Log.Warning("Insufficient stock for {ProductName}", product.Name);
                        return BadRequest($"Insufficient stock for {product.Name}");
                    }
                }

                //  TRANSACTION SAFE
                var hasTransaction = db.Database != null;
                var transaction = hasTransaction ? db.Database.BeginTransaction() : null;

                try
                {
                    // Create order
                    var order = new Order
                    {
                        CustomerId = model.CustomerId,
                        OrderDate = DateTime.Now
                    };

                    db.Orders.Add(order);
                    await db.SaveChangesAsync();

                    // Create order items + update stock
                    foreach (var item in model.Products)
                    {
                        var product = products.First(p => p.Id == item.ProductId);

                        product.StockQuantity -= item.Quantity;

                        db.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price
                        });
                    }

                    await db.SaveChangesAsync();

                    if (hasTransaction)
                        transaction.Commit();

                    return Ok(new
                    {
                        orderId = order.Id,
                        message = "Order created successfully"
                    });
                }
                catch (Exception ex)
                {
                    if (hasTransaction)
                        transaction.Rollback();

                    Log.Error(ex, "Transaction failed while creating order");
                    return InternalServerError();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating order");
                return InternalServerError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}