using System.ComponentModel.DataAnnotations;

namespace InvoiceWebApp.DTOS.Account
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Captcha is required")]
        public string CaptchaToken { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}