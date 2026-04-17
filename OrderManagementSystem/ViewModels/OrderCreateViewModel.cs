using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderManagementSystem.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        public List<ProductSelectionViewModel> Products { get; set; } = new List<ProductSelectionViewModel>();
    }

    public class ProductSelectionViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int AvailableStock { get; set; }  

        public bool IsSelected { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}