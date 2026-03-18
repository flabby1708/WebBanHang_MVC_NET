using SV22T1020497.Models.Partner;

namespace SV22T1020497.Admin.Models
{
    public class CustomerOrdersViewModel
    {
        public Customer Customer { get; set; } = new Customer();
        public List<CustomerOrderItem> Orders { get; set; } = new List<CustomerOrderItem>();
    }
}
