using System;

namespace SV22T1020497.Admin.Models
{
    public class OrderListItem
    {
        public int OrderID { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime OrderTime { get; set; }
        public string DeliveryProvince { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public int Status { get; set; }
    }
}
