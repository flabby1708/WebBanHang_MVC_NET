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
    public class ShipperController : Controller
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
            ViewBag.Title = "Bổ sung người giao hàng";
            return View(new Shipper());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipper model)
        {
            ViewBag.Title = "Bổ sung người giao hàng";

            if (string.IsNullOrWhiteSpace(model.ShipperName))
                ModelState.AddModelError(nameof(model.ShipperName), "Vui lòng nhập tên người giao hàng");

            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.Phone)) model.Phone = "";

            await CreateRepository().AddAsync(model);
            TempData["SuccessMessage"] = "Đã thêm người giao hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật người giao hàng";
            var item = await CreateRepository().GetAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Shipper model)
        {
            ViewBag.Title = "Cập nhật người giao hàng";

            if (string.IsNullOrWhiteSpace(model.ShipperName))
                ModelState.AddModelError(nameof(model.ShipperName), "Vui lòng nhập tên người giao hàng");

            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrWhiteSpace(model.Phone)) model.Phone = "";

            bool updated = await CreateRepository().UpdateAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật người giao hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.Title = "Xóa người giao hàng";
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
                if (await CreateRepository().IsUsedAsync(id))
                {
                    TempData["ErrorMessage"] = "Người giao hàng đang được sử dụng nên chưa thể xóa.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                bool deleted = await CreateRepository().DeleteAsync(id);
                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Không thể xóa người giao hàng.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Người giao hàng đang được sử dụng nên chưa thể xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa người giao hàng.";
            return RedirectToAction(nameof(Index));
        }

        private static ShipperRepository CreateRepository()
        {
            return new ShipperRepository(Configuration.ConnectionString);
        }
    }
}
