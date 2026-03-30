namespace SV22T1020497.Shop.Models
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
    }
}
