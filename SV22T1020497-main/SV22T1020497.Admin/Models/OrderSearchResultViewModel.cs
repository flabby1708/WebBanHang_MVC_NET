using SV22T1020497.Models.Common;
using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Models
{
    public class OrderSearchResultViewModel
    {
        public PagedResult<OrderViewInfo> Data { get; set; } = new();
        public Dictionary<int, decimal> OrderTotals { get; set; } = new();
        public string SearchValue { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
    }
}
