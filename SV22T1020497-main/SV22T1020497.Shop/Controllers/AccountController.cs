using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Models.Partner;
using SV22T1020497.Shop.Models;

namespace SV22T1020497.Shop.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewData["Title"] = "Đăng nhập";
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewData["Title"] = "Đăng nhập";

            if (!ModelState.IsValid)
                return View(model);

            var userData = await SecurityDataService.AuthorizeCustomerAsync(model.UserName, model.Password);
            if (userData == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userData.UserId),
                new(ClaimTypes.Name, userData.DisplayName),
                new(ClaimTypes.Email, userData.Email),
                new(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewData["Title"] = "Đăng ký tài khoản";
            await LoadProvincesAsync();
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["Title"] = "Đăng ký tài khoản";
            await LoadProvincesAsync();

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(model.CustomerName), "Vui lòng nhập tên khách hàng");

            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError(nameof(model.ContactName), "Vui lòng nhập tên giao dịch");

            if (string.IsNullOrWhiteSpace(model.Province))
                ModelState.AddModelError(nameof(model.Province), "Vui lòng chọn tỉnh/thành");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Vui lòng nhập email");
            else if (!await PartnerDataService.ValidatelCustomerEmailAsync(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng");

            if (!ModelState.IsValid)
                return View(model);

            var customer = new Customer
            {
                CustomerName = model.CustomerName.Trim(),
                ContactName = model.ContactName.Trim(),
                Province = model.Province.Trim(),
                Address = model.Address?.Trim() ?? "",
                Phone = model.Phone?.Trim() ?? "",
                Email = model.Email.Trim(),
                Password = model.Password,
                IsLocked = false
            };

            await PartnerDataService.AddCustomerAsync(customer);
            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công. Bạn có thể đăng nhập ngay bây giờ.";
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            ViewData["Title"] = "Thông tin cá nhân";
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return RedirectToAction(nameof(Login));

            await LoadProvincesAsync();
            return View(new CustomerProfileViewModel
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName,
                ContactName = customer.ContactName,
                Province = customer.Province ?? "",
                Address = customer.Address,
                Phone = customer.Phone,
                Email = customer.Email
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfileViewModel model)
        {
            ViewData["Title"] = "Thông tin cá nhân";
            await LoadProvincesAsync();

            var currentCustomer = await GetCurrentCustomerAsync();
            if (currentCustomer == null)
                return RedirectToAction(nameof(Login));

            if (currentCustomer.CustomerID != model.CustomerID)
                return Forbid();

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError(nameof(model.CustomerName), "Vui lòng nhập tên khách hàng");

            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError(nameof(model.ContactName), "Vui lòng nhập tên giao dịch");

            if (string.IsNullOrWhiteSpace(model.Province))
                ModelState.AddModelError(nameof(model.Province), "Vui lòng chọn tỉnh/thành");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Vui lòng nhập email");
            else if (!await PartnerDataService.ValidatelCustomerEmailAsync(model.Email, model.CustomerID))
                ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng");

            if (!ModelState.IsValid)
                return View(model);

            currentCustomer.CustomerName = model.CustomerName.Trim();
            currentCustomer.ContactName = model.ContactName.Trim();
            currentCustomer.Province = model.Province.Trim();
            currentCustomer.Address = model.Address?.Trim() ?? "";
            currentCustomer.Phone = model.Phone?.Trim() ?? "";
            currentCustomer.Email = model.Email.Trim();

            await PartnerDataService.UpdateCustomerAsync(currentCustomer);

            await RefreshSignInAsync(currentCustomer);
            TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Đổi mật khẩu";
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewData["Title"] = "Đổi mật khẩu";

            if (!ModelState.IsValid)
                return View(model);

            string email = User.FindFirstValue(ClaimTypes.Email) ?? "";
            var currentUser = await SecurityDataService.AuthorizeCustomerAsync(email, model.OldPassword);
            if (currentUser == null)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            bool result = await SecurityDataService.ChangeCustomerPasswordAsync(email, model.NewPassword);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Không thể đổi mật khẩu. Vui lòng thử lại sau.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private async Task LoadProvincesAsync()
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "", Text = "-- Chọn tỉnh/thành --" }
            };

            var provinces = await DictionaryDataService.ListProvincesAsync();
            foreach (var item in provinces)
            {
                items.Add(new SelectListItem
                {
                    Value = item.ProvinceName,
                    Text = item.ProvinceName
                });
            }

            ViewBag.Provinces = items;
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            if (!int.TryParse(userId, out int customerId))
                return null;

            return await PartnerDataService.GetCustomerAsync(customerId);
        }

        private async Task RefreshSignInAsync(Customer customer)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                new(ClaimTypes.Name, customer.CustomerName),
                new(ClaimTypes.Email, customer.Email),
                new(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true
                });
        }
    }
}
