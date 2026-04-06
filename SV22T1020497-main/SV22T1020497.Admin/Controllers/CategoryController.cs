using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.AppCodes;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;

namespace SV22T1020497.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Products},{WebUserRoles.SystemAdmin}")]
    public class CategoryController : Controller
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
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            await CreateRepository().AddAsync(model);
            TempData["SuccessMessage"] = "Đã thêm loại hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await CreateRepository().GetAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            bool updated = await CreateRepository().UpdateAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật loại hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await CreateRepository().GetAsync(id);
            if (item == null) return NotFound();
            return View(item);
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
                    TempData["ErrorMessage"] = "Không thể xóa loại hàng.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Loại hàng đang được sử dụng nên chưa thể xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa loại hàng.";
            return RedirectToAction(nameof(Index));
        }

        private static CategoryRepository CreateRepository()
        {
            return new CategoryRepository(Configuration.ConnectionString);
        }
    }
}
