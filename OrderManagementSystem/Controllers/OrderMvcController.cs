using OrderManagementSystem.Models;
using OrderManagementSystem.Models.DTOs;
using OrderManagementSystem.Models.ViewModels;
using OrderManagementSystem.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OrderManagementSystem.Controllers
{
    public class OrderMvcController : Controller
    {
        private readonly ApiClientService _api;

        public OrderMvcController()
        {
            _api = new ApiClientService();
        }

        // GET: OrderMvc
        public async Task<ActionResult> Index()
        {
            try
            {
                Log.Information("Fetching orders");
                var orders = await _api.GetAsync<List<OrderDto>>("api/orders");

                if (orders == null)
                {
                    ViewBag.Error = "Failed to load orders";
                    return View(new List<OrderDto>());
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading orders");
                ViewBag.Error = "Something went wrong";
                return View(new List<OrderDto>());
            }
        }

        // GET: OrderMvc/Create
        public async Task<ActionResult> Create()
        {
            try
            {
                Log.Information("Loading order create page");
                var customers = await _api.GetAsync<List<CustomerDto>>("api/customers");
                var products = await _api.GetAsync<List<ProductDto>>("api/products");

                if (customers == null || products == null)
                {
                    TempData["Error"] = "Failed to load data for order creation";
                    return RedirectToAction("Index");
                }

                var model = new OrderCreateViewModel
                {
                    Products = products.Select(p => new ProductSelectionViewModel
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        AvailableStock = p.StockQuantity,
                        Quantity = 1
                    }).ToList()
                };

                ViewBag.Customers = new SelectList(customers, "Id", "Name");
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading order create page");
                TempData["Error"] = "Error loading page";
                return RedirectToAction("Index");
            }
        }

        // POST: OrderMvc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OrderCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ReloadDropdowns(model);
                await ReloadProducts(model);        
                return View(model);
            }

            var selectedProducts = model.Products?
                .Where(p => p.IsSelected)
                .Select(p => new ProductSelection { ProductId = p.ProductId, Quantity = p.Quantity })
                .ToList();

            if (selectedProducts == null || !selectedProducts.Any())
            {
                ModelState.AddModelError("", "Please select at least one product");
                await ReloadDropdowns(model);
                return View(model);
            }

            try
            {
                var orderRequest = new OrderViewModel
                {
                    CustomerId = model.CustomerId,
                    Products = selectedProducts
                };

                var (success, error) = await _api.PostAsyncWithError("api/orders", orderRequest);

                if (success)
                {
                    TempData["Success"] = "Order created successfully";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", error ?? "Failed to create order");
                await ReloadDropdowns(model);
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating order");
                ModelState.AddModelError("", "Unexpected error occurred");
                await ReloadDropdowns(model);
                return View(model);
            }
        }

        // GET: OrderMvc/Details/id
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                Log.Information("Loading order details: {OrderId}", id);
                var order = await _api.GetAsync<OrderDetailDto>($"api/orders/{id}");

                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction("Index");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading order details: {OrderId}", id);
                TempData["Error"] = "Error loading order details";
                return RedirectToAction("Index");
            }
        }

        private async Task ReloadDropdowns(OrderCreateViewModel model)
        {
            try
            {
                Log.Information("Reloading dropdown data for Order Create");
                var customers = await _api.GetAsync<List<CustomerDto>>("api/customers");
                ViewBag.Customers = new SelectList(customers ?? new List<CustomerDto>(), "Id", "Name", model.CustomerId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reloading dropdowns");
                ViewBag.Customers = new SelectList(new List<CustomerDto>(), "Id", "Name");
            }
        }

        private async Task ReloadProducts(OrderCreateViewModel model)
        {
            var products = await _api.GetAsync<List<ProductDto>>("api/products");

            model.Products = products.Select(p => new ProductSelectionViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                AvailableStock = p.StockQuantity,

                // to preserve user input after 
                Quantity = model.Products?
                    .FirstOrDefault(x => x.ProductId == p.Id)?.Quantity ?? 1,

                IsSelected = model.Products?
                    .FirstOrDefault(x => x.ProductId == p.Id)?.IsSelected ?? false
            }).ToList();
        }
    }
}


