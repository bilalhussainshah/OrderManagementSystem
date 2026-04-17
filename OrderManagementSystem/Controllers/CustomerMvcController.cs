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
    public class CustomerMvcController : Controller
    {
        private readonly ApiClientService _api;

        public CustomerMvcController()
        {
            _api = new ApiClientService();
        }

        // GET: CustomerMvc
        public async Task<ActionResult> Index()
        {
            try
            {
                Log.Information("Fetching customer list");
                var customers = await _api.GetAsync<List<CustomerDto>>("api/customers");

                if (customers == null)
                {
                    Log.Warning("Customer list is null");
                    ViewBag.Error = "Failed to load customers. Please try again.";
                    return View(new List<CustomerDto>());
                }

                return View(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Customer Index");
                ViewBag.Error = "Something went wrong";
                return View(new List<CustomerDto>());
            }
        }

        // GET: CustomerMvc/Create
        public ActionResult Create()
        {
            return View(new CustomerViewModel());
        }

        // POST: CustomerMvc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CustomerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new CustomerDto { Name = model.Name, Email = model.Email };
                Log.Information("Creating customer: {Name}", model.Name);
                var (success, error) = await _api.PostAsyncWithError("api/customers", dto);

                if (success)
                {
                    TempData["Success"] = "Customer created successfully";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", error ?? "Failed to create customer");
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating customer");
                ModelState.AddModelError("", "Unexpected error occurred");
                return View(model);
            }
        }

        // GET: CustomerMvc/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                Log.Information("Loading customer for edit: {CustomerId}", id);
                var customer = await _api.GetAsync<CustomerDto>($"api/customers/{id}");
                if (customer == null)
                {
                    Log.Warning("Customer not found: {CustomerId}", id);
                    return HttpNotFound();
                }

                return View(new CustomerViewModel
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading customer for edit: {CustomerId}", id);
                TempData["Error"] = "Error loading customer";
                return RedirectToAction("Index");
            }
        }

        // POST: CustomerMvc/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CustomerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new CustomerDto { Id = id, Name = model.Name, Email = model.Email };
                Log.Information("Updating customer: {CustomerId}", id);
                var (success, error) = await _api.PutAsyncWithError($"api/customers/{id}", dto);

                if (success)
                {
                    TempData["Success"] = "Customer updated successfully";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", error ?? "Update failed");
                return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating customer: {CustomerId}", id);
                ModelState.AddModelError("", "Unexpected error");
                return View(model);
            }
        }

        // GET: CustomerMvc/Details/id
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var customer = await _api.GetAsync<CustomerDto>($"api/customers/{id}");
                if (customer == null) return HttpNotFound();

                return View(customer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading customer details: {Id}", id);
                TempData["Error"] = "Error loading details";
                return RedirectToAction("Index");
            }
        }

        /*       // GET: CustomerMvc/ConfirmDelete/5
               public async Task<ActionResult> ConfirmDelete(int id)
               {
                   try
                   {
                       var customer = await _api.GetAsync<CustomerDto>($"api/customers/{id}");
                       if (customer == null) return HttpNotFound();

                       return View(customer);
                   }
                   catch (Exception ex)
                   {
                       Log.Error(ex, "Error loading customer for delete confirmation: {Id}", id);
                       TempData["Error"] = "Error loading customer";
                       return RedirectToAction("Index");
                   }
               }

               // POST: CustomerMvc/Delete/5
               [HttpPost]
               [ValidateAntiForgeryToken]
               [ActionName("Delete")]
               public async Task<ActionResult> DeleteConfirmed(int id)
               {
                   try
                   {
                       var (success, error) = await _api.DeleteAsyncWithError($"api/customers/{id}");

                       if (success)
                           TempData["Success"] = "Customer deleted successfully";
                       else
                           TempData["Error"] = error ?? "Delete failed";

                       return RedirectToAction("Index");
                   }
                   catch (Exception ex)
                   {
                       Log.Error(ex, "Error deleting customer: {Id}", id);
                       TempData["Error"] = "Unexpected error";
                       return RedirectToAction("Index");
                   }
               }*/

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                Log.Information("Deleting customer: {CustomerId}", id);

                var success = await _api.DeleteAsync($"api/customers/{id}");

                TempData[success ? "Success" : "Error"] =
                    success ? "Customer deleted successfully" : "Delete failed";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting customer: {CustomerId}", id);
                TempData["Error"] = "Unexpected error";
                return RedirectToAction("Index");
            }
        }


    }
}