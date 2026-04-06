using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.AppCodes;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;

namespace SV22T1020497.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Products},{WebUserRoles.SystemAdmin}")]
    public class SupplierController : Controller
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

        public IActionResult Create() => View(new Supplier());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier model)
        {
            if (!ModelState.IsValid) return View(model);

            await CreateRepository().AddAsync(model);
            TempData["SuccessMessage"] = "Đã thêm nhà cung cấp từ cơ sở dữ liệu.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await CreateRepository().GetAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier model)
        {
            if (!ModelState.IsValid) return View(model);

            bool updated = await CreateRepository().UpdateAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật thông tin nhà cung cấp.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await CreateRepository().GetAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                bool deleted = await CreateRepository().DeleteAsync(id);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhà cung cấp.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Nhà cung cấp đang được sử dụng nên chưa thể xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa nhà cung cấp.";
            return RedirectToAction(nameof(Index));
        }

        private static SupplierRepository CreateRepository()
        {
            return new SupplierRepository(Configuration.ConnectionString);
        }
    }
}
