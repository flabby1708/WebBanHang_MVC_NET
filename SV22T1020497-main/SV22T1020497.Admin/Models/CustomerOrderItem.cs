namespace SV22T1020497.Admin.Models
{
    public class CustomerOrderItem
    {
        public int OrderID { get; set; }
        public DateTime OrderedTime { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}
