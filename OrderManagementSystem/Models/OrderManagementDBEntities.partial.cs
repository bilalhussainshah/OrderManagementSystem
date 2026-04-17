using System.Data.Entity;

namespace OrderManagementSystem.Models
{
    public partial class OrderManagementDBEntities : IOrderManagementDBEntities
    {

        IDbSet<Customer> IOrderManagementDBEntities.Customers
        {
            get { return this.Customers; }
        }

        IDbSet<Product> IOrderManagementDBEntities.Products
        {
            get { return this.Products; }
        }

        IDbSet<Order> IOrderManagementDBEntities.Orders
        {
            get { return this.Orders; }
        }

        IDbSet<OrderItem> IOrderManagementDBEntities.OrderItems
        {
            get { return this.OrderItems; }
        }
    }
}