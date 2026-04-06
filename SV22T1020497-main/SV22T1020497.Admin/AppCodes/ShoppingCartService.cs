using SV22T1020497.Models.Sales;

namespace SV22T1020497.Admin.AppCodes
{
    /// <summary>
    /// Cung cap cac chuc nang xu ly tren gio hang
    /// (Gio hang luu trong session)
    /// </summary>
    public static class ShoppingCartService
    {
        /// <summary>
        /// Ten bien de luu gio hang trong session
        /// </summary>
        private const string CART = "ShoppingCart";

        /// <summary>
        /// Lay gio hang tu session
        /// </summary>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }

            return cart;
        }

        /// <summary>
        /// Lay thong tin 1 mat hang tu gio hang
        /// </summary>
        public static OrderDetailViewInfo? GetCartItem(int productID)
        {
            var cart = GetShoppingCart();
            return cart.Find(m => m.ProductID == productID);
        }

        /// <summary>
        /// Them hang vao gio hang
        /// </summary>
        public static void AddCartItem(OrderDetailViewInfo item)
        {
            var cart = GetShoppingCart();
            var existsItem = cart.Find(m => m.ProductID == item.ProductID);
            if (existsItem == null)
            {
                cart.Add(item);
            }
            else
            {
                existsItem.Quantity += item.Quantity;
                existsItem.SalePrice = item.SalePrice;
            }

            ApplicationContext.SetSessionData(CART, cart);
        }

        /// <summary>
        /// Cap nhat so luong va gia cua mot mat hang trong gio hang
        /// </summary>
        public static void UpdateCartItem(int productID, int quantity, decimal salePrice)
        {
            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID);
            if (item != null)
            {
                item.Quantity = quantity;
                item.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        /// <summary>
        /// Xoa mot mat hang ra khoi gio hang
        /// </summary>
        public static void RemoveCartItem(int productID)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productID);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData(CART, cart);
            }
        }

        /// <summary>
        /// Xoa gio hang
        /// </summary>
        public static void ClearCart()
        {
            var cart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData(CART, cart);
        }
    }
}
