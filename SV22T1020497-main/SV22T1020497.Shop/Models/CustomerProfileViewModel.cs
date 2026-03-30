using System.ComponentModel.DataAnnotations;

namespace SV22T1020497.Shop.Models
{
    public class CustomerProfileViewModel
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng")]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên giao dịch")]
        [Display(Name = "Tên giao dịch")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành")]
        [Display(Name = "Tỉnh/thành")]
        public string Province { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Điện thoại")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
