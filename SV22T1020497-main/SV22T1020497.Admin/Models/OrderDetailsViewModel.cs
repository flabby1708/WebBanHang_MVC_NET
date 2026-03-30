using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.Models
{
    public class OrderDetailsViewModel
    {
        public OrderViewInfo Order { get; set; } = new();
        public List<OrderDetailViewInfo> Details { get; set; } = new();
        public decimal TotalAmount => Details.Sum(x => x.TotalPrice);
    }
}
