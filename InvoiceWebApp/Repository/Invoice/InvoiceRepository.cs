using Microsoft.EntityFrameworkCore;
using InvoiceWebApp.Data;
using InvoiceWebApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceWebApp.Repository.Invoice
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Models.Invoice> GetAllByUserId(int userId)
        {
            return _context.Invoices
                .Where(i => i.UserId == userId)
                .Include(i => i.InvoiceDetails)
                .OrderByDescending(i => i.Date)
                .ToList();
        }

        public Models.Invoice? GetById(int id)
        {
            return _context.Invoices
                .Include(i => i.InvoiceDetails)
                 .Include(i => i.User)
                .FirstOrDefault(i => i.Id == id);
        }

        public Models.Invoice Create(Models.Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
            return invoice;
        }

        public Models.Invoice Update(Models.Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            _context.SaveChanges();
            return invoice;
        }

        public void Delete(int id)
        {
            var invoice = GetById(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                _context.SaveChanges();
            }
        }
    }
}
