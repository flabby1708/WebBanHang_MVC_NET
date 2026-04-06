using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020497.BusinessLayers;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;
using SV22T1020497.Shop.Models;
using System.Globalization;
using System.Text;

namespace SV22T1020497.Shop.Controllers
{
    public class ProductController : Controller
    {
        private static string NormalizeProductName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString()
                .Replace('đ', 'd')
                .Replace('Đ', 'd');
        }

        public async Task<IActionResult> Index(string searchValue = "", int categoryId = 0, decimal minPrice = 0, decimal maxPrice = 0, int page = 1)
        {
            ViewData["Title"] = "Sản phẩm";

            var input = new ProductSearchInput
            {
                SearchValue = searchValue,
                CategoryID = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SupplierID = 0,
                Page = page,
                PageSize = 12
            };

            var fullInput = new ProductSearchInput
            {
                SearchValue = input.SearchValue,
                CategoryID = input.CategoryID,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice,
                SupplierID = input.SupplierID,
                Page = 1,
                PageSize = 0
            };

            var allProducts = await CatalogDataService.ListProductsAsync(fullInput);
            var distinctProducts = allProducts.DataItems
                .Where(x => x.IsSelling)
                .GroupBy(x => NormalizeProductName(x.ProductName))
                .Select(g => g.OrderBy(x => x.ProductID).First())
                .OrderBy(x => x.ProductName)
                .ToList();

            int currentPage = Math.Max(input.Page, 1);
            var result = new PagedResult<Product>
            {
                Page = currentPage,
                PageSize = input.PageSize,
                RowCount = distinctProducts.Count,
                DataItems = distinctProducts
                    .Skip((currentPage - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList()
            };

            var model = new ProductListViewModel
            {
                SearchInput = input,
                Data = result,
                Categories = await LoadCategoriesAsync(categoryId)
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null || !product.IsSelling)
                return NotFound();

            ViewData["Title"] = product.ProductName;

            var photos = await CatalogDataService.ListPhotosAsync(id);
            var attributes = await CatalogDataService.ListAttributesAsync(id);

            return View(new ProductDetailsViewModel
            {
                Product = product,
                Photos = photos.Where(x => !x.IsHidden).ToList(),
                Attributes = attributes
            });
        }

        private static async Task<List<SelectListItem>> LoadCategoriesAsync(int selectedValue)
        {
            var items = new List<SelectListItem>
            {
                new() { Value = "0", Text = "Tất cả loại hàng" }
            };

            var categories = await CatalogDataService.ListCategoriesAsync(new PaginationSearchInput
            {
                Page = 1,
                PageSize = 0,
                SearchValue = ""
            });

            items.AddRange(categories.DataItems.Select(x => new SelectListItem
            {
                Value = x.CategoryID.ToString(),
                Text = x.CategoryName,
                Selected = x.CategoryID == selectedValue
            }));

            return items;
        }
    }
}
