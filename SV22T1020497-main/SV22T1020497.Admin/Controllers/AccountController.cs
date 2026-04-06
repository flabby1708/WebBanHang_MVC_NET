using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.AppCodes;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Admin.Models;

namespace SV22T1020497.Admin.Controllers
{
    public class AccountController : Controller
    {
        private static List<string> NormalizeRoles(string? roleNames)
        {
            var roles = string.IsNullOrWhiteSpace(roleNames)
                ? new List<string>()
                : roleNames
                    .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            bool hasAdmin = roles.Any(r => string.Equals(r, WebUserRoles.Administrator, StringComparison.OrdinalIgnoreCase));
            bool hasEmployee = roles.Any(r => string.Equals(r, "employee", StringComparison.OrdinalIgnoreCase));

            if (hasAdmin && !roles.Contains(WebUserRoles.SystemAdmin, StringComparer.OrdinalIgnoreCase))
                roles.Add(WebUserRoles.SystemAdmin);

            // Tai khoan role cu "employee" duoc phep nhin cac nhom chuc nang co ban de tuong thich voi du lieu seed cu.
            if (hasEmployee || hasAdmin)
            {
                if (!roles.Contains(WebUserRoles.Customers, StringComparer.OrdinalIgnoreCase))
                    roles.Add(WebUserRoles.Customers);
                if (!roles.Contains(WebUserRoles.Products, StringComparer.OrdinalIgnoreCase))
                    roles.Add(WebUserRoles.Products);
                if (!roles.Contains(WebUserRoles.Orders, StringComparer.OrdinalIgnoreCase))
                    roles.Add(WebUserRoles.Orders);
            }

            return roles;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var account = await SecurityDataService.AuthorizeEmployeeAsync(model.UserName, model.Password);
            if (account == null)
            {
                string hashedPassword = CryptHelper.HashMD5(model.Password);
                account = await SecurityDataService.AuthorizeEmployeeAsync(model.UserName, hashedPassword);
            }

            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            var userData = new WebUserData
            {
                UserId = account.UserId,
                UserName = account.UserName,
                DisplayName = string.IsNullOrWhiteSpace(account.DisplayName) ? account.UserName : account.DisplayName,
                Email = account.Email,
                Photo = account.Photo,
                Roles = NormalizeRoles(account.RoleNames)
            };

            var principal = userData.CreatePrincipal();

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

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.Title = "Đổi mật khẩu";
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewBag.Title = "Đổi mật khẩu";

            if (!ModelState.IsValid)
                return View(model);

            var userData = User.GetUserData();
            string email = userData?.Email ?? userData?.UserName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            var currentAccount = await SecurityDataService.AuthorizeEmployeeAsync(email, model.OldPassword);
            if (currentAccount == null)
            {
                string hashedOldPassword = CryptHelper.HashMD5(model.OldPassword);
                currentAccount = await SecurityDataService.AuthorizeEmployeeAsync(email, hashedOldPassword);
            }

            if (currentAccount == null)
            {
                ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            string hashedNewPassword = CryptHelper.HashMD5(model.NewPassword);
            bool result = await SecurityDataService.ChangeEmployeePasswordAsync(email, hashedNewPassword);

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Không thể đổi mật khẩu vào lúc này.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
