using SV22T1020497.Models.Catalog;

namespace SV22T1020497.Shop.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; } = new();
        public List<ProductAttribute> Attributes { get; set; } = new();
        public List<ProductPhoto> Photos { get; set; } = new();
        public int Quantity { get; set; } = 1;
    }
}
