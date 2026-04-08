using SV22T1020497.Models.Sales;

namespace SV22T1020497.Shop.Models
{
    public class OrderDetailsViewModel
    {
        public OrderViewInfo Order { get; set; } = new();
        public List<OrderDetailViewInfo> Details { get; set; } = new();
        public decimal TotalAmount => Details.Sum(x => x.TotalPrice);
        public List<OrderTimelineItem> Timeline => BuildTimeline();

        private List<OrderTimelineItem> BuildTimeline()
        {
            var result = new List<OrderTimelineItem>
            {
                new()
                {
                    Title = "Đơn hàng được tạo",
                    Description = "Hệ thống đã ghi nhận yêu cầu mua hàng của bạn.",
                    Time = Order.OrderTime,
                    IsCompleted = true,
                    IsCurrent = Order.Status == OrderStatusEnum.New
                },
                new()
                {
                    Title = "Đơn hàng được duyệt",
                    Description = "Nhân viên xác nhận và chuẩn bị xử lý đơn.",
                    Time = Order.AcceptTime,
                    IsCompleted = Order.AcceptTime.HasValue,
                    IsCurrent = Order.Status == OrderStatusEnum.Accepted
                },
                new()
                {
                    Title = "Đang giao hàng",
                    Description = string.IsNullOrWhiteSpace(Order.ShipperName)
                        ? "Đơn hàng đang trên quá trình vận chuyển."
                        : $"Người giao: {Order.ShipperName}",
                    Time = Order.ShippedTime,
                    IsCompleted = Order.ShippedTime.HasValue,
                    IsCurrent = Order.Status == OrderStatusEnum.Shipping
                }
            };

            string finalTitle = Order.Status switch
            {
                OrderStatusEnum.Completed => "Giao hàng thành công",
                OrderStatusEnum.Cancelled => "Đơn hàng đã hủy",
                OrderStatusEnum.Rejected => "Đơn hàng bị từ chối",
                _ => "Hoàn tất đơn hàng"
            };

            string finalDescription = Order.Status switch
            {
                OrderStatusEnum.Completed => "Bạn đã nhận hàng thành công.",
                OrderStatusEnum.Cancelled => "Đơn hàng đã được hủy trong quá trình xử lý.",
                OrderStatusEnum.Rejected => "Đơn hàng không được xác nhận từ hệ thống.",
                _ => "Đơn hàng chưa hoàn tất."
            };

            result.Add(new OrderTimelineItem
            {
                Title = finalTitle,
                Description = finalDescription,
                Time = Order.FinishedTime,
                IsCompleted = Order.Status is OrderStatusEnum.Completed or OrderStatusEnum.Cancelled or OrderStatusEnum.Rejected,
                IsCurrent = Order.Status is OrderStatusEnum.Completed or OrderStatusEnum.Cancelled or OrderStatusEnum.Rejected
            });

            return result;
        }
    }

    public class OrderTimelineItem
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? Time { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
    }
}
