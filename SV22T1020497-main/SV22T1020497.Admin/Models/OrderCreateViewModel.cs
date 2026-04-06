using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1020497.Models.Catalog;
using SV22T1020497.Models.Common;
using SV22T1020497.Models.Partner;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Models
{
    public class OrderCreateViewModel
    {
        public string SearchValue { get; set; } = "";
        public int Page { get; set; } = 1;
        public PagedResult<Product> Products { get; set; } = new();
        public List<OrderDetailViewInfo> Cart { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public List<SelectListItem> Provinces { get; set; } = new();
        public int? CustomerID { get; set; }
        public string? DeliveryProvince { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal CartTotal => Cart.Sum(item => item.TotalPrice);
        public List<PageItem> DisplayPages => Products.GetDisplayPages(2);
    }
}
