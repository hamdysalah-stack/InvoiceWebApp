using InvoiceWebApp.Models;
using System.Collections.Generic;

namespace InvoiceWebApp.Repository.Invoice
{
    public interface IInvoiceRepository
    {
        IEnumerable<Models.Invoice> GetAllByUserId(int userId);
        Models.Invoice? GetById(int id);
        Models.Invoice Create(Models.Invoice invoice);
        Models.Invoice Update(Models.Invoice invoice);
        void Delete(int id);
    }
}
