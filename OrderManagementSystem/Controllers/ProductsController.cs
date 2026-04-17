using OrderManagementSystem.Models;
using OrderManagementSystem.Models.DTOs;
using Serilog;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace OrderManagementSystem.Controllers
{
    public class ProductsController : ApiController
    {

        private readonly IOrderManagementDBEntities db;

        public ProductsController()
        {
            db = new OrderManagementDBEntities();
        }

        public ProductsController(IOrderManagementDBEntities db)
        {
            this.db = db;
        }
        
        //private readonly OrderManagementDBEntities db = new OrderManagementDBEntities();

        // GET api/products
        public async Task<IHttpActionResult> GetProducts()
        {
            try
            {
                var products = await db.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity
                    })
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching products");
                return InternalServerError(ex);
            }
        }

        // GET api/products/id
        public async Task<IHttpActionResult> GetProduct(int id)
        {
            try
            {
                var product = await db.Products
    .FirstOrDefaultAsync(x => x.Id == id);
                if (product == null) return NotFound();

                return Ok(new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching product {Id}", id);
                return InternalServerError(ex);
            }
        }

        // POST api/products
        public async Task<IHttpActionResult> PostProduct(ProductDto dto)
        {
            if (dto == null)
                return BadRequest("Product data required");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Price <= 0)
                return BadRequest("Price must be greater than 0");

            if (dto.StockQuantity < 0)
                return BadRequest("Stock quantity cannot be negative");

            try
            {
                var product = new Product
                {
                    Name = dto.Name.Trim(),
                    Price = dto.Price,
                    StockQuantity = dto.StockQuantity
                };

                db.Products.Add(product);
                await db.SaveChangesAsync();

                dto.Id = product.Id;
                Log.Information("Product created: {Id} {Name}", product.Id, product.Name);

                return Ok(dto);
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "DB error creating product");
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        // PUT api/products/id
        public async Task<IHttpActionResult> PutProduct(int id, ProductDto dto)
        {
            if (dto == null)
                return BadRequest("Product data required");

            if (dto.Id != 0 && dto.Id != id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Price <= 0)
                return BadRequest("Price must be greater than 0");

            if (dto.StockQuantity < 0)
                return BadRequest("Stock cannot be negative");

            try
            {
                var product = await db.Products
    .FirstOrDefaultAsync(x => x.Id == id);
                if (product == null) return NotFound();

                product.Name = dto.Name.Trim();
                product.Price = dto.Price;
                product.StockQuantity = dto.StockQuantity;

                await db.SaveChangesAsync();
                Log.Information("Product updated: {Id}", id);

                return Ok("Product updated successfully");
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product {Id}", id);
                return InternalServerError(ex);
            }
        }

        // DELETE api/products/id
        public async Task<IHttpActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await db.Products
    .FirstOrDefaultAsync(x => x.Id == id);
                if (product == null) return NotFound();

                bool usedInOrders = await db.OrderItems.AnyAsync(oi => oi.ProductId == id);
                if (usedInOrders)
                    return BadRequest("Cannot delete a product that is part of existing orders");

                db.Products.Remove(product);
                await db.SaveChangesAsync();

                Log.Information("Product deleted: {Id}", id);
                return Ok("Product deleted successfully");
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "DB error deleting product {Id}", id);
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}