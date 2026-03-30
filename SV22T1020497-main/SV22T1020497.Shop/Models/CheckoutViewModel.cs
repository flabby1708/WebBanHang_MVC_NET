using System.ComponentModel.DataAnnotations;

namespace SV22T1020497.Shop.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành giao hàng")]
        [Display(Name = "Tỉnh/thành giao hàng")]
        public string DeliveryProvince { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DeliveryAddress { get; set; } = string.Empty;

        public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
    }
}
