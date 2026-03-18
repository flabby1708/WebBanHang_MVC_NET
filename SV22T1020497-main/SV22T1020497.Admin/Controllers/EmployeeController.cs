using Microsoft.AspNetCore.Mvc;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.HR;

namespace SV22T1020497.Admin.Controllers
{
    public class EmployeeController : Controller
    {
        public async Task<IActionResult> Index(string searchValue = "", int page = 1)
        {
            var input = new PaginationSearchInput
            {
                SearchValue = searchValue,
                Page = page,
                PageSize = 5
            };

            var result = await CreateRepository().ListAsync(input);
            ViewBag.SearchValue = searchValue;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.PageCount;

            return View(result.DataItems);
        }

        public IActionResult Create()
        {
            return View(new Employee());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model)
        {
            if (!ModelState.IsValid) return View(model);

            var repository = CreateRepository();
            if (!await repository.ValidateEmailAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                return View(model);
            }

            await repository.AddAsync(model);
            TempData["SuccessMessage"] = "Đã thêm nhân viên.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var emp = await CreateRepository().GetAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee model)
        {
            if (!ModelState.IsValid) return View(model);

            var repository = CreateRepository();
            if (!await repository.ValidateEmailAsync(model.Email, model.EmployeeID))
            {
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                return View(model);
            }

            bool updated = await repository.UpdateAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật thông tin nhân viên.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var emp = await CreateRepository().GetAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                bool deleted = await CreateRepository().DeleteAsync(id);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhân viên.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Nhân viên đang có dữ liệu liên quan nên chưa thể xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa nhân viên.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangePassword(int id)
        {
            var emp = await CreateRepository().GetAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string newPassword, string confirmPassword)
        {
            var repository = CreateRepository();
            var emp = await repository.GetAsync(id);
            if (emp == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu không hợp lệ hoặc không khớp.";
                return RedirectToAction(nameof(ChangePassword), new { id });
            }

            emp.Password = newPassword;
            await repository.UpdateAsync(emp);
            TempData["SuccessMessage"] = "Đã đổi mật khẩu.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangeRole(int id)
        {
            var emp = await CreateRepository().GetAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string[] roles)
        {
            var repository = CreateRepository();
            var emp = await repository.GetAsync(id);
            if (emp == null) return NotFound();

            emp.RoleNames = roles == null
                ? ""
                : string.Join(",", roles.Where(r => !string.IsNullOrWhiteSpace(r)));

            await repository.UpdateAsync(emp);
            TempData["SuccessMessage"] = "Đã cập nhật phân quyền.";
            return RedirectToAction(nameof(Index));
        }

        private static EmployeeRepository CreateRepository()
        {
            return new EmployeeRepository(Configuration.ConnectionString);
        }
    }
}
