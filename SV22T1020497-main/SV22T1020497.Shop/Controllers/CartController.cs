using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;
using SV22T1020497.Models.Sales;
using SV22T1020497.Shop.AppCodes;
using SV22T1020497.Shop.Models;

namespace SV22T1020497.Shop.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Giỏ hàng";
            return View(new CartViewModel
            {
                Items = CartService.GetCart()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null || !product.IsSelling)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                return RedirectToAction("Index", "Product");
            }

            CartService.AddItem(new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? "",
                Quantity = quantity <= 0 ? 1 : quantity,
                SalePrice = product.Price
            });

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            CartService.UpdateItem(productId, quantity);
            TempData["SuccessMessage"] = "Giỏ hàng đã được cập nhật.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            CartService.RemoveItem(productId);
            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            var cart = CartService.GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            CartService.Clear();
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = CartService.GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Đặt mua hàng";
            await LoadProvincesAsync();

            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return RedirectToAction("Login", "Account");

            return View(new CheckoutViewModel
            {
                Items = cart,
                DeliveryProvince = customer.Province ?? "",
                DeliveryAddress = customer.Address ?? ""
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            ViewData["Title"] = "Đặt mua hàng";
            await LoadProvincesAsync();

            var cart = CartService.GetCart();
            model.Items = cart;

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(model.DeliveryProvince))
                ModelState.AddModelError(nameof(model.DeliveryProvince), "Vui lòng chọn tỉnh/thành giao hàng");

            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                ModelState.AddModelError(nameof(model.DeliveryAddress), "Vui lòng nhập địa chỉ giao hàng");

            if (!ModelState.IsValid)
                return View(model);

            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return RedirectToAction("Login", "Account");

            var orderId = await SalesDataService.AddOrderAsync(new Order
            {
                CustomerID = customer.CustomerID,
                DeliveryProvince = model.DeliveryProvince.Trim(),
                DeliveryAddress = model.DeliveryAddress.Trim()
            });

            foreach (var item in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                });
            }

            CartService.Clear();
            TempData["SuccessMessage"] = "Đơn hàng của bạn đã được tạo thành công.";
            return RedirectToAction("Details", "Order", new { id = orderId });
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
    }
}
