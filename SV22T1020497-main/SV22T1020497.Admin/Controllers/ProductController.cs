using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Admin.AppCodes;

namespace SV22T1020497.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.Products},{WebUserRoles.SystemAdmin}")]
    public class ProductController : Controller
    {
        public async Task<IActionResult> Index(string searchValue = "", int page = 1)
        {
            var input = new ProductSearchInput
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
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid) return View(model);

            await CreateRepository().AddAsync(model);
            TempData["SuccessMessage"] = "Đã thêm mặt hàng.";
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
        public async Task<IActionResult> Edit(Product model)
        {
            if (!ModelState.IsValid) return View(model);

            bool updated = await CreateRepository().UpdateAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật mặt hàng.";
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
                    TempData["ErrorMessage"] = "Không thể xóa mặt hàng.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Mặt hàng đang phát sinh dữ liệu liên quan nên chưa thể xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = "Đã xóa mặt hàng.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ListAttributes(int id)
        {
            ViewBag.Product = await CreateRepository().GetAsync(id);
            return View(await CreateRepository().ListAttributesAsync(id));
        }

        public IActionResult CreateAttribute(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAttribute(int id, ProductAttribute model)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        public IActionResult EditAttribute(int id, int attributeId)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAttribute(int id, ProductAttribute model)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        public IActionResult DeleteAttribute(int id, int attributeId)
        {
            return View();
        }

        [HttpPost, ActionName("DeleteAttribute")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAttributeConfirmed(int id, int attributeId)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> ListPhotos(int id)
        {
            ViewBag.Product = await CreateRepository().GetAsync(id);
            return View(await CreateRepository().ListPhotosAsync(id));
        }

        public IActionResult CreatePhoto(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePhoto(int id, ProductPhoto model)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        public IActionResult EditPhoto(int id, int photoId)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPhoto(int id, ProductPhoto model)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        public IActionResult DeletePhoto(int id, int photoId)
        {
            return View();
        }

        [HttpPost, ActionName("DeletePhoto")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePhotoConfirmed(int id, int photoId)
        {
            return RedirectToAction(nameof(Edit), new { id });
        }

        private static ProductRepository CreateRepository()
        {
            return new ProductRepository(Configuration.ConnectionString);
        }
    }
}
