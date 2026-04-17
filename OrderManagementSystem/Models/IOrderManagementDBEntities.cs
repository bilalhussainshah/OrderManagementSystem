using System.Data.Entity;
using System.Threading.Tasks;

namespace OrderManagementSystem.Models
{
    public interface IOrderManagementDBEntities
    {
        IDbSet<Customer> Customers { get; }
        IDbSet<Product> Products { get; }
        IDbSet<Order> Orders { get; }
        IDbSet<OrderItem> OrderItems { get; }

        Task<int> SaveChangesAsync();
        Database Database { get; }
        void Dispose();
    }
}