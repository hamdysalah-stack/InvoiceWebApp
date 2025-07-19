using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace InvoiceWebApp.DTOS.InvoiceDTO
{
    public class CreateInvoiceDTO
    {
        [Required(ErrorMessage = "Invoice date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "At least one invoice detail is required")]
        [MinLength(1, ErrorMessage = "An invoice must have at least one detail")]
        public List<InvoiceDetailDTO> InvoiceDetails { get; set; } = new List<InvoiceDetailDTO>();
    }
}