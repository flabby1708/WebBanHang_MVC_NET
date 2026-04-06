using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.AppCodes;
using SV22T1020497.Admin.Models;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Orders},{WebUserRoles.SystemAdmin}")]
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

        public async Task<IActionResult> Create(string searchValue = "", int page = 1)
        {
            return View(await BuildCreateViewModelAsync(searchValue, page));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateViewModel model)
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (!cart.Any())
                ModelState.AddModelError("", "Giỏ hàng đang trống, chưa thể lập đơn hàng.");
            if (!model.CustomerID.HasValue || model.CustomerID.Value <= 0)
                ModelState.AddModelError(nameof(model.CustomerID), "Vui lòng chọn khách hàng.");
            if (string.IsNullOrWhiteSpace(model.DeliveryProvince))
                ModelState.AddModelError(nameof(model.DeliveryProvince), "Vui lòng chọn tỉnh/thành giao hàng.");
            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                ModelState.AddModelError(nameof(model.DeliveryAddress), "Vui lòng nhập địa chỉ giao hàng.");

            SaveDraft(model);

            var customer = model.CustomerID.HasValue
                ? await PartnerDataService.GetCustomerAsync(model.CustomerID.Value)
                : null;
            if (model.CustomerID.HasValue && customer == null)
                ModelState.AddModelError(nameof(model.CustomerID), "Khách hàng không tồn tại.");
            else if (customer != null)
            {
                var customerProvince = customer.Province?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(customerProvince))
                    ModelState.AddModelError(nameof(model.CustomerID), "Khách hàng này chưa có tỉnh/thành trong dữ liệu.");
                else
                    model.DeliveryProvince = customerProvince;
            }

            if (cart.Any(item => item.Quantity <= 0 || item.SalePrice <= 0))
                ModelState.AddModelError("", "Giỏ hàng có dữ liệu không hợp lệ. Vui lòng kiểm tra lại số lượng và giá bán.");

            if (!ModelState.IsValid)
                return View(await BuildCreateViewModelAsync(model.SearchValue, model.Page, model));

            var orderId = await SalesDataService.AddOrderAsync(new Order
            {
                CustomerID = model.CustomerID,
                DeliveryProvince = model.DeliveryProvince?.Trim(),
                DeliveryAddress = model.DeliveryAddress?.Trim()
            });

            if (orderId <= 0)
            {
                ModelState.AddModelError("", "Không thể tạo đơn hàng.");
                return View(await BuildCreateViewModelAsync(model.SearchValue, model.Page, model));
            }

            foreach (var item in cart)
            {
                var success = await SalesDataService.AddDetailAsync(new OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                });

                if (!success)
                {
                    TempData["ErrorMessage"] = "Đơn hàng đã được tạo nhưng có lỗi khi lưu chi tiết đơn hàng.";
                    return RedirectToAction(nameof(Detail), new { id = orderId });
                }
            }

            ShoppingCartService.ClearCart();
            OrderDraftService.ClearDraft();
            TempData["SuccessMessage"] = "Đã lập đơn hàng thành công.";
            return RedirectToAction(nameof(Detail), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity, decimal salePrice, string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction(nameof(Create), new { searchValue, page });
            }

            if (salePrice <= 0)
            {
                TempData["ErrorMessage"] = "Giá bán phải lớn hơn 0.";
                return RedirectToAction(nameof(Create), new { searchValue, page });
            }

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Mặt hàng không tồn tại.";
                return RedirectToAction(nameof(Create), new { searchValue, page });
            }

            ShoppingCartService.AddCartItem(new OrderDetailViewInfo
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo ?? "",
                Quantity = quantity,
                SalePrice = salePrice
            });

            TempData["SuccessMessage"] = "Đã thêm mặt hàng vào giỏ.";
            return RedirectToAction(nameof(Create), new { searchValue, page });
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

        public IActionResult EditCartItem(int productId, string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);

            var item = ShoppingCartService.GetCartItem(productId);
            if (item == null)
                return Content("<div class='modal-body text-danger'>Không tìm thấy mặt hàng trong giỏ.</div>", "text/html");

            ViewBag.SearchValue = searchValue;
            ViewBag.Page = page;
            ViewBag.CustomerID = customerId;
            ViewBag.DeliveryProvince = deliveryProvince;
            ViewBag.DeliveryAddress = deliveryAddress;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCartItem(int productId, int quantity, decimal salePrice, string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);

            if (quantity <= 0 || salePrice <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng và giá bán phải lớn hơn 0.";
                return RedirectToAction(nameof(Create), new { searchValue, page });
            }

            ShoppingCartService.UpdateCartItem(productId, quantity, salePrice);
            TempData["SuccessMessage"] = "Đã cập nhật mặt hàng trong giỏ.";
            return RedirectToAction(nameof(Create), new { searchValue, page });
        }

        public IActionResult DeleteCartItem(int productId, string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);

            var item = ShoppingCartService.GetCartItem(productId);
            if (item == null)
                return Content("<div class='modal-body text-danger'>Không tìm thấy mặt hàng trong giỏ.</div>", "text/html");

            ViewBag.SearchValue = searchValue;
            ViewBag.Page = page;
            ViewBag.CustomerID = customerId;
            ViewBag.DeliveryProvince = deliveryProvince;
            ViewBag.DeliveryAddress = deliveryAddress;
            return View(item);
        }

        [HttpPost, ActionName("DeleteCartItem")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCartItemConfirmed(int productId, string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);
            ShoppingCartService.RemoveCartItem(productId);
            TempData["SuccessMessage"] = "Đã xóa mặt hàng khỏi giỏ.";
            return RedirectToAction(nameof(Create), new { searchValue, page });
        }

        public IActionResult ClearCart(string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);
            ViewBag.SearchValue = searchValue;
            ViewBag.Page = page;
            ViewBag.CartItemCount = ShoppingCartService.GetShoppingCart().Count;
            ViewBag.CustomerID = customerId;
            ViewBag.DeliveryProvince = deliveryProvince;
            ViewBag.DeliveryAddress = deliveryAddress;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCartConfirmed(string searchValue = "", int page = 1, int? customerId = null, string? deliveryProvince = null, string? deliveryAddress = null)
        {
            SaveDraft(customerId, deliveryProvince, deliveryAddress);
            ShoppingCartService.ClearCart();
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction(nameof(Create), new { searchValue, page });
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

        private static async Task<OrderCreateViewModel> BuildCreateViewModelAsync(string searchValue, int page, OrderCreateViewModel? source = null)
        {
            var draft = source == null
                ? OrderDraftService.GetDraft()
                : new OrderCreateDraft
                {
                    CustomerID = source.CustomerID,
                    DeliveryProvince = source.DeliveryProvince,
                    DeliveryAddress = source.DeliveryAddress
                };

            var productInput = new ProductSearchInput
            {
                SearchValue = searchValue ?? "",
                Page = page < 1 ? 1 : page,
                PageSize = 5
            };

            var products = await CatalogDataService.ListProductsAsync(productInput);
            var customers = await PartnerDataService.ListCustomersAsync(new PaginationSearchInput
            {
                SearchValue = "",
                Page = 1,
                PageSize = 0
            });

            return new OrderCreateViewModel
            {
                SearchValue = searchValue ?? "",
                Page = products.Page,
                Products = products,
                Cart = ShoppingCartService.GetShoppingCart(),
                Customers = customers.DataItems,
                Provinces = await SelectListHelper.Provinces(),
                CustomerID = draft.CustomerID,
                DeliveryProvince = draft.DeliveryProvince,
                DeliveryAddress = draft.DeliveryAddress
            };
        }

        private static void SaveDraft(OrderCreateViewModel model)
        {
            SaveDraft(model.CustomerID, model.DeliveryProvince, model.DeliveryAddress);
        }

        private static void SaveDraft(int? customerId, string? deliveryProvince, string? deliveryAddress)
        {
            OrderDraftService.SaveDraft(new OrderCreateDraft
            {
                CustomerID = customerId,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress
            });
        }
    }
}
