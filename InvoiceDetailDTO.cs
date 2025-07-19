using System.ComponentModel.DataAnnotations;

namespace InvoiceWebApp.DTOS.InvoiceDTO
{
    public class InvoiceDetailDTO
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot be longer than 200 characters")]
        public string Product { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        public decimal Total => Quantity * Price;
    }
}
