using System;

namespace OrderManagementSystem.Models.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public string CustomerName { get; set; }

        public decimal Total { get; set; }
    }
}