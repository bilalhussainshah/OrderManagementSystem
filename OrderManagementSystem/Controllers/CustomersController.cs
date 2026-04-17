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
    public class CustomersController : ApiController
    {

        private readonly IOrderManagementDBEntities db;

        public CustomersController()
        {
            db = new OrderManagementDBEntities();
        }

        public CustomersController(IOrderManagementDBEntities db)
        {
            this.db = db;
        }

        //private readonly OrderManagementDBEntities db = new OrderManagementDBEntities();

        // GET api/customers
        public async Task<IHttpActionResult> GetCustomers()
        {
            try
            {
                var customers = await db.Customers
                    .OrderBy(c => c.Name)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        CreatedAt = c.CreatedAt
                    }).ToListAsync();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching customers");
                return InternalServerError(ex);
            }
        }

        // GET api/customers/id
        public async Task<IHttpActionResult> GetCustomer(int id)
        {
            try
            {
                var c = await db.Customers
    .FirstOrDefaultAsync(x => x.Id == id);
                if (c == null) return NotFound();

                return Ok(new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    CreatedAt = c.CreatedAt
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching customer {Id}", id);
                return InternalServerError(ex);
            }
        }

        // POST api/customers
        public async Task<IHttpActionResult> PostCustomer(CustomerDto dto)
        {
            if (dto == null)
                return BadRequest("Customer data required");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email
            bool emailExists = await db.Customers.AnyAsync(c => c.Email == dto.Email);
            if (emailExists)
                return BadRequest("A customer with this email already exists");

            try
            {
                var customer = new Customer
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    CreatedAt = DateTime.Now
                };

                db.Customers.Add(customer);
                await db.SaveChangesAsync();

                dto.Id = customer.Id;
                dto.CreatedAt = customer.CreatedAt;

                Log.Information("Customer created: {Id} {Email}", customer.Id, customer.Email);
                return Ok(dto);
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "DB error creating customer");
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/customers/id
        public async Task<IHttpActionResult> PutCustomer(int id, CustomerDto dto)
        {
            if (dto == null || id != dto.Id)
                return BadRequest("Invalid customer data or Customer data required");

            // Allow dto.Id to be 0 (not sent) — use route id as authority
            if (dto.Id != 0 && dto.Id != id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var customer = await db.Customers
    .FirstOrDefaultAsync(x => x.Id == id);
                if (customer == null) return NotFound();

                // Check duplicate email (excluding self)
                bool emailTaken = await db.Customers.AnyAsync(c => c.Email == dto.Email && c.Id != id);
                if (emailTaken)
                    return BadRequest("This email is already used by another customer");

                customer.Name = dto.Name.Trim();
                customer.Email = dto.Email.Trim().ToLower();

                await db.SaveChangesAsync();
                Log.Information("Customer updated: {Id}", id);

                return Ok("Customer updated successfully");
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (DbUpdateException ex)
            {
                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating customer {Id}", id);
                return InternalServerError(ex);
            }
        }

        // DELETE api/customers/id
        public async Task<IHttpActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await db.Customers
    .FirstOrDefaultAsync(x => x.Id == id);
                if (customer == null) return NotFound();

                // Prevent deletion if customer has orders
                bool hasOrders = await db.Orders.AnyAsync(o => o.CustomerId == id);
                if (hasOrders)
                    return BadRequest("Cannot delete customer with existing orders");

                db.Customers.Remove(customer);
                await db.SaveChangesAsync();

                Log.Information("Customer deleted: {Id}", id);
                return Ok("Customer deleted successfully");
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "DB error deleting customer {Id}", id);
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