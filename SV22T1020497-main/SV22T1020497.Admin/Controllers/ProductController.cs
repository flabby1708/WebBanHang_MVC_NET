using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020497.Admin.AppCodes;
using SV22T1020497.BusinessLayers;
using SV22T1020497.DataLayers.SQLServer;
using SV22T1020497.Models.Catalog;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
            var product = await CreateRepository().GetAsync(id);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View(await CreateRepository().ListAttributesAsync(id));
        }

        public async Task<IActionResult> CreateAttribute(int id)
        {
            var product = await CreateRepository().GetAsync(id);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View(new ProductAttribute
            {
                ProductID = id,
                DisplayOrder = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAttribute(int id, ProductAttribute model)
        {
            var product = await CreateRepository().GetAsync(id);
            if (product == null) return NotFound();

            model.ProductID = id;
            ValidateAttribute(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }

            await CreateRepository().AddAttributeAsync(model);
            TempData["SuccessMessage"] = "Đã bổ sung thuộc tính.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> EditAttribute(int id, int attributeId)
        {
            var repository = CreateRepository();
            var product = await repository.GetAsync(id);
            if (product == null) return NotFound();

            var attribute = await repository.GetAttributeAsync(attributeId);
            if (attribute == null || attribute.ProductID != id) return NotFound();

            ViewBag.Product = product;
            return View(attribute);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAttribute(int id, ProductAttribute model)
        {
            var repository = CreateRepository();
            var product = await repository.GetAsync(id);
            if (product == null) return NotFound();

            model.ProductID = id;
            ValidateAttribute(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }

            bool updated = await repository.UpdateAttributeAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật thuộc tính.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> DeleteAttribute(int id, int attributeId)
        {
            var repository = CreateRepository();
            var product = await repository.GetAsync(id);
            if (product == null) return NotFound();

            var attribute = await repository.GetAttributeAsync(attributeId);
            if (attribute == null || attribute.ProductID != id) return NotFound();

            ViewBag.Product = product;
            return View(attribute);
        }

        [HttpPost, ActionName("DeleteAttribute")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttributeConfirmed(int id, int attributeId)
        {
            bool deleted = await CreateRepository().DeleteAttributeAsync(attributeId);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
                ? "Đã xóa thuộc tính."
                : "Không thể xóa thuộc tính.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> ListPhotos(int id)
        {
            var product = await CreateRepository().GetAsync(id);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View(await CreateRepository().ListPhotosAsync(id));
        }

        public async Task<IActionResult> CreatePhoto(int id)
        {
            var product = await CreateRepository().GetAsync(id);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View(new ProductPhoto
            {
                ProductID = id,
                DisplayOrder = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePhoto(int id, ProductPhoto model, IFormFile? uploadPhoto)
        {
            var product = await CreateRepository().GetAsync(id);
            if (product == null) return NotFound();

            model.ProductID = id;
            if (uploadPhoto == null && string.IsNullOrWhiteSpace(model.Photo))
                ModelState.AddModelError(nameof(model.Photo), "Vui lòng chọn ảnh hoặc nhập tên file ảnh.");

            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }

            if (uploadPhoto != null)
                model.Photo = await SaveProductPhotoAsync(product.ProductName, uploadPhoto);

            await CreateRepository().AddPhotoAsync(model);
            TempData["SuccessMessage"] = "Đã bổ sung ảnh mặt hàng.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> EditPhoto(int id, int photoId)
        {
            var repository = CreateRepository();
            var product = await repository.GetAsync(id);
            if (product == null) return NotFound();

            var photo = await repository.GetPhotoAsync(photoId);
            if (photo == null || photo.ProductID != id) return NotFound();

            ViewBag.Product = product;
            return View(photo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoto(int id, ProductPhoto model, IFormFile? uploadPhoto)
        {
            var repository = CreateRepository();
            var product = await repository.GetAsync(id);
            if (product == null) return NotFound();

            var currentPhoto = await repository.GetPhotoAsync(model.PhotoID);
            if (currentPhoto == null || currentPhoto.ProductID != id) return NotFound();

            model.ProductID = id;
            if (uploadPhoto == null && string.IsNullOrWhiteSpace(model.Photo))
                ModelState.AddModelError(nameof(model.Photo), "Vui lòng chọn ảnh hoặc nhập tên file ảnh.");

            if (!ModelState.IsValid)
            {
                ViewBag.Product = product;
                return View(model);
            }

            if (uploadPhoto != null)
                model.Photo = await SaveProductPhotoAsync(product.ProductName, uploadPhoto, currentPhoto.Photo);

            bool updated = await repository.UpdatePhotoAsync(model);
            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Đã cập nhật ảnh mặt hàng.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> DeletePhoto(int id, int photoId)
        {
            var repository = CreateRepository();
            var product = await repository.GetAsync(id);
            if (product == null) return NotFound();

            var photo = await repository.GetPhotoAsync(photoId);
            if (photo == null || photo.ProductID != id) return NotFound();

            ViewBag.Product = product;
            return View(photo);
        }

        [HttpPost, ActionName("DeletePhoto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhotoConfirmed(int id, int photoId)
        {
            var repository = CreateRepository();
            var photo = await repository.GetPhotoAsync(photoId);
            if (photo != null && photo.ProductID == id)
                DeleteProductPhotoFile(photo.Photo);

            bool deleted = await repository.DeletePhotoAsync(photoId);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
                ? "Đã xóa ảnh mặt hàng."
                : "Không thể xóa ảnh mặt hàng.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        private static ProductRepository CreateRepository()
        {
            return new ProductRepository(Configuration.ConnectionString);
        }

        private void ValidateAttribute(ProductAttribute model)
        {
            if (string.IsNullOrWhiteSpace(model.AttributeName))
                ModelState.AddModelError(nameof(model.AttributeName), "Vui lòng nhập tên thuộc tính.");

            if (string.IsNullOrWhiteSpace(model.AttributeValue))
                ModelState.AddModelError(nameof(model.AttributeValue), "Vui lòng nhập giá trị thuộc tính.");
        }

        private static async Task<string> SaveProductPhotoAsync(string? productName, IFormFile uploadPhoto, string? currentPhoto = null)
        {
            string fileName = BuildProductPhotoFileName(productName, uploadPhoto.FileName);
            string directory = Path.Combine(ApplicationContext.WWWRootPath, "images", "products");
            Directory.CreateDirectory(directory);

            DeleteOldProductPhoto(directory, currentPhoto, fileName);

            string filePath = Path.Combine(directory, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await uploadPhoto.CopyToAsync(stream);

            return fileName;
        }

        private static void DeleteProductPhotoFile(string? photo)
        {
            if (string.IsNullOrWhiteSpace(photo))
                return;

            string fileName = Path.GetFileName(photo);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            string filePath = Path.Combine(ApplicationContext.WWWRootPath, "images", "products", fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        private static string BuildProductPhotoFileName(string? productName, string originalFileName)
        {
            string extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            string normalizedName = NormalizeFileName(productName);
            if (string.IsNullOrWhiteSpace(normalizedName))
                normalizedName = $"product-{Guid.NewGuid():N}";

            return normalizedName + extension.ToLowerInvariant();
        }

        private static void DeleteOldProductPhoto(string directory, string? currentPhoto, string newFileName)
        {
            if (string.IsNullOrWhiteSpace(currentPhoto))
                return;

            string currentFileName = Path.GetFileName(currentPhoto);
            if (string.IsNullOrWhiteSpace(currentFileName) ||
                string.Equals(currentFileName, newFileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string oldPath = Path.Combine(directory, currentFileName);
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
