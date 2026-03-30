using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.Models;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string searchValue = "", int status = 0, string? dateFrom = null, string? dateTo = null, int page = 1)
        {
            var input = new OrderSearchInput
            {
                SearchValue = searchValue ?? "",
                Status = ParseStatus(status),
                DateFrom = ParseDate(dateFrom),
                DateTo = ParseDate(dateTo),
                Page = page,
                PageSize = 10
            };

            var data = await SalesDataService.ListOrdersAsync(input);
            var totals = new Dictionary<int, decimal>();
            foreach (var item in data.DataItems)
            {
                var details = await SalesDataService.ListDetailsAsync(item.OrderID);
                totals[item.OrderID] = details.Sum(x => x.TotalPrice);
            }

            return View(new OrderSearchResultViewModel
            {
                Data = data,
                OrderTotals = totals,
                SearchValue = searchValue ?? "",
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo
            });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order model)
        {
            return View(model);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction(nameof(Index));

            var details = await SalesDataService.ListDetailsAsync(id);
            return View(new OrderDetailsViewModel
            {
                Order = order,
                Details = details
            });
        }

        public IActionResult EditCartItem(int id, int productId)
        {
            return View();
        }

        public IActionResult DeleteCartItem(int id, int productId)
        {
            return View();
        }

        public IActionResult ClearCart()
        {
            return View();
        }

        public IActionResult Accept(int id)
        {
            return View();
        }

        public IActionResult Shipping(int id)
        {
            return View();
        }

        public IActionResult Finish(int id)
        {
            return View();
        }

        public IActionResult Reject(int id)
        {
            return View();
        }

        public IActionResult Cancel(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            return View();
        }

        private static DateTime? ParseDate(string? value)
        {
            return DateTime.TryParse(value, out var d) ? d : null;
        }

        private static OrderStatusEnum ParseStatus(int value)
        {
            return Enum.IsDefined(typeof(OrderStatusEnum), value)
                ? (OrderStatusEnum)value
                : (OrderStatusEnum)0;
        }
    }
}
