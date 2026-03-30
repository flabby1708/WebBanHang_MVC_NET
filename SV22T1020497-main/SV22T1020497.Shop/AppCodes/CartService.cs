using SV22T1020497.Shop.Models;

namespace SV22T1020497.Shop.AppCodes
{
    public static class CartService
    {
        public const string ShoppingCartKey = "shopping_cart";

        public static List<CartItem> GetCart()
        {
            return ApplicationContext.GetSessionData<List<CartItem>>(ShoppingCartKey) ?? new List<CartItem>();
        }

        public static void SaveCart(List<CartItem> items)
        {
            ApplicationContext.SetSessionData(ShoppingCartKey, items);
        }

        public static void AddItem(CartItem item)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.ProductID == item.ProductID);
            if (existing == null)
            {
                cart.Add(item);
            }
            else
            {
                existing.Quantity += item.Quantity;
            }
            SaveCart(cart);
        }

        public static void UpdateItem(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);
            if (item == null)
                return;

            if (quantity <= 0)
                cart.Remove(item);
            else
                item.Quantity = quantity;

            SaveCart(cart);
        }

        public static void RemoveItem(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
        }

        public static void Clear()
        {
            SaveCart(new List<CartItem>());
        }

        public static int CountItems()
        {
            return GetCart().Sum(x => x.Quantity);
        }
    }
}
