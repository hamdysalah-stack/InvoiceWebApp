using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using InvoiceWebApp.DTOS.Account;
using InvoiceWebApp.Services;


namespace InvoiceWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public AccountController(UserService userService, EmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        private string GenerateCaptchaCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            HttpContext.Session.SetString("CaptchaCode", result);
            return result;
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.SetString("CaptchaCode", GenerateCaptchaCode());
            ViewBag.Captcha = HttpContext.Session.GetString("CaptchaCode");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginDTO model)
        {
            ViewBag.Captcha = HttpContext.Session.GetString("CaptchaCode");

            var storedCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (string.IsNullOrEmpty(storedCaptcha) || !string.Equals(model.CaptchaToken, storedCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("CaptchaToken", "Invalid Captcha code.");
                HttpContext.Session.SetString("CaptchaCode", GenerateCaptchaCode());
                return View(model);
            }
            HttpContext.Session.Remove("CaptchaCode");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = _userService.Authenticate(model.Username, model.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    HttpContext.Session.SetString("CaptchaCode", GenerateCaptchaCode());
                    return View(model);
                }

                if (!user.IsEmailVerified)
                {
                    ModelState.AddModelError("", "Please verify your email before logging in.");
                    HttpContext.Session.SetString("CaptchaCode", GenerateCaptchaCode());
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(60)
                };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties).Wait();

                return RedirectToAction("Index", "Invoice");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred during login.");
                HttpContext.Session.SetString("CaptchaCode", GenerateCaptchaCode());
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _userService.Register(model);
                TempData["Message"] = "Registration successful! Please check your email to verify your account.";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred during registration.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.VerificationStatus = "Error";
                ViewBag.VerificationMessage = "Invalid verification link. The token is missing.";
                return View(); 
            }

            var result = _userService.VerifyEmail(token);

            if (result)
            {
                ViewBag.VerificationStatus = "Success";
                ViewBag.VerificationMessage = "Email verified successfully! You can now log in.";
            }
            else
            {
                ViewBag.VerificationStatus = "Error";
                ViewBag.VerificationMessage = "Email verification failed. The token might be invalid or expired.";
            }
            return View(); 
        }
      

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}