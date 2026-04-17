using OrderManagementSystem.Models.DTOs;
using OrderManagementSystem.Models.ViewModels;
using OrderManagementSystem.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OrderManagementSystem.Controllers
{
    public class ProductMvcController : Controller
    {
        private readonly ApiClientService _api;

        public ProductMvcController()
        {
            _api = new ApiClientService();
        }

        // GET: ProductMvc
        public async Task<ActionResult> Index()
        {
            try
            {
                Log.Information("Fetching product list");

                var products = await _api.GetAsync<List<ProductDto>>("api/products");
                if (products == null)
                {
                    Log.Warning("Product list is null");
                    ViewBag.Error = "Failed to load products";
                    return View(new List<ProductDto>());
                }

                return View(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Product Index");
                ViewBag.Error = "Something went wrong";
                return View(new List<ProductDto>());
            }
        }

        // GET: ProductMvc/Create
        public ActionResult Create()
        {
            return View(new ProductViewModel());
        }

        // POST: ProductMvc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new ProductDto { Name = model.Name, Price = model.Price, StockQuantity = model.Stock };
                var (success, error) = await _api.PostAsyncWithError("api/products", dto);

                if (success)
                {
                    TempData["Success"] = "Product created successfully";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", error ?? "Failed to create product");
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating product");
                ModelState.AddModelError("", "Unexpected error occurred");
                return View(model);
            }
        }

        // GET: ProductMvc/Edit/id
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                Log.Information("Loading product for edit: {ProductId}", id);
                var product = await _api.GetAsync<ProductDto>($"api/products/{id}");
                if (product == null)
                {
                    Log.Warning("Product not found: {ProductId}", id);
                    return HttpNotFound();
                }

                return View(new ProductViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Stock = product.StockQuantity
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading product for edit: {ProductId}", id);
                TempData["Error"] = "Error loading product";
                return RedirectToAction("Index");
            }
        }

        // POST: ProductMvc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new ProductDto { Id = id, Name = model.Name, Price = model.Price, StockQuantity = model.Stock };
                Log.Information("Updating product: {ProductId}", id);
                var (success, error) = await _api.PutAsyncWithError($"api/products/{id}", dto);

                if (success)
                {
                    TempData["Success"] = "Product updated successfully";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", error ?? "Update failed");
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating product: {ProductId}", id);
                ModelState.AddModelError("", "Unexpected error");
                return View(model);
            }
        }

        
        // DELETE
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                Log.Information("Deleting product: {ProductId}", id);

                var success = await _api.DeleteAsync($"api/products/{id}");

                TempData[success ? "Success" : "Error"] =
                    success ? "Product deleted successfully" : "Delete failed";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting product: {ProductId}", id);
                TempData["Error"] = "Unexpected error";
                return RedirectToAction("Index");
            }
        }

        // DETAILS
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                Log.Information("Deleting product: {ProductId}", id);
                var product = await _api.GetAsync<ProductDto>($"api/products/{id}");
                if (product == null) return HttpNotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading product details: {Id}", id);
                TempData["Error"] = "Error loading details";
                return RedirectToAction("Index");
            }
        }


    }
}