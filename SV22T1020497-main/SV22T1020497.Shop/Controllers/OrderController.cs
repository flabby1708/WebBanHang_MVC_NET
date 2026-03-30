using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Shop.Models;

namespace SV22T1020497.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Đơn hàng của tôi";
            int customerId = GetCurrentCustomerId();
            if (customerId <= 0)
                return RedirectToAction("Login", "Account");

            return View(new OrderListViewModel
            {
                Orders = await SalesDataService.ListOrdersByCustomerAsync(customerId)
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            int customerId = GetCurrentCustomerId();
            if (customerId <= 0)
                return RedirectToAction("Login", "Account");

            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != customerId)
                return NotFound();

            ViewData["Title"] = $"Đơn hàng #{order.OrderID}";
            return View(new OrderDetailsViewModel
            {
                Order = order,
                Details = await SalesDataService.ListDetailsAsync(id)
            });
        }

        private int GetCurrentCustomerId()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            return int.TryParse(userId, out int customerId) ? customerId : 0;
        }
    }
}
