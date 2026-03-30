using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;

namespace SV22T1020497.Shop.Models
{
    public class ProductListViewModel
    {
        public ProductSearchInput SearchInput { get; set; } = new();
        public PagedResult<Product> Data { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
    }
}
