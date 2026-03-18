using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.Models;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Controllers
{
    public class CustomerController : Controller
    {
        public async Task<IActionResult> Index(string searchValue = "", int page = 1, int pageSize = 10)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = pageSize
            };

            var result = await CreateCustomerRepository().ListAsync(input);

            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.PageCount;
            ViewBag.TotalItems = result.RowCount;
            ViewBag.DisplayPages = result.GetDisplayPages(2);

            return View(result.DataItems);
        }

        public IActionResult Create()
        {
            return View(new Customer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid) return View(model);

            var repository = CreateCustomerRepository();
            if (!await repository.ValidateEmailAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                return View(model);
            }

            model.CustomerID = await repository.AddAsync(model);
            TempData["SuccessMessage"] = "Đã thêm khách hàng từ cơ sở dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await CreateCustomerRepository().GetAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer model)
        {
            if (!ModelState.IsValid) return View(model);

            var repository = CreateCustomerRepository();
            if (!await repository.ValidateEmailAsync(model.Email, model.CustomerID))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                return View(model);
            }

            bool updated = await repository.UpdateAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật khách hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await CreateCustomerRepository().GetAsync(id);
            if (customer == null) return NotFound();

            var orders = await GetCustomerOrdersAsync(id);
            var model = new CustomerDetailsViewModel
            {
                Customer = customer,
                TotalOrders = orders.Count,
                TotalSpent = orders.Sum(o => o.TotalAmount),
                LastOrderTime = orders.OrderByDescending(o => o.OrderedTime).FirstOrDefault()?.OrderedTime
            };

            return View(model);
        }

        public async Task<IActionResult> Orders(int id)
        {
            var customer = await CreateCustomerRepository().GetAsync(id);
            if (customer == null) return NotFound();

            var model = new CustomerOrdersViewModel
            {
                Customer = customer,
                Orders = await GetCustomerOrdersAsync(id)
            };

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await CreateCustomerRepository().GetAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var repository = CreateCustomerRepository();
            var customer = await repository.GetAsync(id);
            if (customer == null) return NotFound();

            try
            {
                bool deleted = await repository.DeleteAsync(id);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Khách hàng đang có dữ liệu liên quan nên chưa thể xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa khách hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangePassword(int id)
        {
            var customer = await CreateCustomerRepository().GetAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var repository = CreateCustomerRepository();
            var customer = await repository.GetAsync(id);
            if (customer == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("", "Mật khẩu không được để trống");
                return View(customer);
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Xác nhận mật khẩu không trùng khớp");
                return View(customer);
            }

            customer.Password = newPassword;
            await repository.UpdateAsync(customer);

            TempData["SuccessMessage"] = "Đã thay đổi mật khẩu thành công";
            return RedirectToAction(nameof(Index));
        }

        private static CustomerRepository CreateCustomerRepository()
        {
            return new CustomerRepository(Configuration.ConnectionString);
        }

        private static OrderRepository CreateOrderRepository()
        {
            return new OrderRepository(Configuration.ConnectionString);
        }

        private static async Task<List<CustomerOrderItem>> GetCustomerOrdersAsync(int customerId)
        {
            var orderRepository = CreateOrderRepository();
            var orders = await orderRepository.ListByCustomerAsync(customerId);
            var result = new List<CustomerOrderItem>();

            foreach (var order in orders)
            {
                var details = await orderRepository.ListDetailsAsync(order.OrderID);
                result.Add(new CustomerOrderItem
                {
                    OrderID = order.OrderID,
                    OrderedTime = order.OrderTime,
                    EmployeeName = order.EmployeeName,
                    Status = order.Status.GetDescription(),
                    TotalAmount = details.Sum(d => d.Quantity * d.SalePrice)
                });
            }

            return result;
        }
    }
}
