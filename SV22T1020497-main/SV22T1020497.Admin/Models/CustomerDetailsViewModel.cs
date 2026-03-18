using SV22T1020497.Models.Partner;

namespace SV22T1020497.Admin.Models
{
    public class CustomerDetailsViewModel
    {
        public Customer Customer { get; set; } = new Customer();
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderTime { get; set; }
    }
}
