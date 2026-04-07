using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.AppCodes;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
        public async Task<IActionResult> Create(Category model, IFormFile? uploadPhoto)
        {
            if (!ModelState.IsValid) return View(model);

            if (uploadPhoto != null)
            {
                var fileName = BuildCategoryPhotoFileName(model.CategoryName, uploadPhoto.FileName);
                var directory = Path.Combine(ApplicationContext.WWWRootPath, "images", "categories");
                Directory.CreateDirectory(directory);

                var filePath = Path.Combine(directory, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                model.Photo = fileName;
            }

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
        public async Task<IActionResult> Edit(Category model, IFormFile? uploadPhoto)
        {
            if (!ModelState.IsValid) return View(model);

            if (uploadPhoto != null)
            {
                var fileName = BuildCategoryPhotoFileName(model.CategoryName, uploadPhoto.FileName);
                var directory = Path.Combine(ApplicationContext.WWWRootPath, "images", "categories");
                Directory.CreateDirectory(directory);

                DeleteOldCategoryPhoto(directory, model.Photo, fileName);

                var filePath = Path.Combine(directory, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                model.Photo = fileName;
            }

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

        private static string BuildCategoryPhotoFileName(string? categoryName, string originalFileName)
        {
            string extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            string normalizedName = NormalizeFileName(categoryName);
            if (string.IsNullOrWhiteSpace(normalizedName))
                normalizedName = $"category-{Guid.NewGuid():N}";

            return normalizedName + extension.ToLowerInvariant();
        }

        private static void DeleteOldCategoryPhoto(string directory, string? currentPhoto, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(currentPhoto))
                return;

            if (string.Equals(currentPhoto, newFileName, StringComparison.OrdinalIgnoreCase))
                return;

            string oldPath = Path.Combine(directory, currentPhoto);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        private static string NormalizeFileName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }

            string result = builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('\u0111', 'd');

            return Regex.Replace(result, @"[^a-z0-9]+", "-").Trim('-');
        }
    }
}
