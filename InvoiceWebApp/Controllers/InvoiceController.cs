using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using InvoiceWebApp.Services;
using InvoiceWebApp.DTOS.InvoiceDTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceWebApp.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly InvoiceService _invoiceService;

        public InvoiceController(InvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new InvalidOperationException("User ID not found in claims or is invalid.");
            }
            return userId;
        }

        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            var invoices = _invoiceService.GetUserInvoices(userId);
            return View(invoices);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateInvoiceDTO
            {
                Date = DateTime.Today,
                InvoiceDetails = new List<InvoiceDetailDTO>
                {
                    new InvoiceDetailDTO()
                }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateInvoiceDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.InvoiceDetails == null || !model.InvoiceDetails.Any() || model.InvoiceDetails.All(d => string.IsNullOrWhiteSpace(d.Product) && d.Quantity == 0 && d.Price == 0))
            {
                ModelState.AddModelError("InvoiceDetails", "At least one valid product detail is required.");
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId();
                _invoiceService.CreateInvoice(userId, model);

                TempData["Message"] = "Invoice created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred while creating the invoice: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var invoice = _invoiceService.GetInvoiceById(id);

            if (invoice == null)
                return NotFound();

            if (invoice.UserId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(invoice);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var invoice = _invoiceService.GetInvoiceById(id);

            if (invoice == null)
                return NotFound();

            if (invoice.UserId != GetCurrentUserId())
            {
                return Forbid();
            }

            var model = new CreateInvoiceDTO
            {
                Date = invoice.Date,
                InvoiceDetails = invoice.InvoiceDetails.Select(d => new InvoiceDetailDTO
                {
                    Product = d.Product,
                    Quantity = d.Quantity,
                    Price = d.Price
                }).ToList()
            };

            if (!model.InvoiceDetails.Any())
            {
                model.InvoiceDetails.Add(new InvoiceDetailDTO());
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CreateInvoiceDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.InvoiceDetails == null || !model.InvoiceDetails.Any() || model.InvoiceDetails.All(d => string.IsNullOrWhiteSpace(d.Product) && d.Quantity == 0 && d.Price == 0))
            {
                ModelState.AddModelError("InvoiceDetails", "At least one valid product detail is required.");
                return View(model);
            }

            try
            {
                var existingInvoice = _invoiceService.GetInvoiceById(id);
                if (existingInvoice == null || existingInvoice.UserId != GetCurrentUserId())
                {
                    return NotFound();
                }

                _invoiceService.UpdateInvoice(id, model);
                TempData["Message"] = "Invoice updated successfully!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred while updating the invoice: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var invoiceToDelete = _invoiceService.GetInvoiceById(id);
                if (invoiceToDelete == null || invoiceToDelete.UserId != GetCurrentUserId())
                {
                    return NotFound();
                }

                _invoiceService.DeleteInvoice(id);
                TempData["Message"] = "Invoice deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting the invoice: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}