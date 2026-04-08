using SV22T1020497.Models.Sales;

namespace SV22T1020497.Shop.Models
{
    public class OrderListViewModel
    {
        public List<OrderViewInfo> Orders { get; set; } = new();
        public int TotalOrders => Orders.Count;
        public int PendingOrders => Orders.Count(x => x.Status == OrderStatusEnum.New || x.Status == OrderStatusEnum.Accepted);
        public int ShippingOrders => Orders.Count(x => x.Status == OrderStatusEnum.Shipping);
        public int CompletedOrders => Orders.Count(x => x.Status == OrderStatusEnum.Completed);
    }
}
