using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagementSystem.Models
{
    public class OrderViewModel
    {
        public int CustomerId { get; set; }
        public List<ProductSelection> Products { get; set; }
    }

    public class ProductSelection
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}