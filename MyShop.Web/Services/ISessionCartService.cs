using MyShop.Entities.Models;

namespace MyShop.Web.Services
{
    public interface ISessionCartService
    {
        List<CartItem> GetCartItems();
        void AddToCart(int productId, int count);
        void UpdateCartItem(int productId, int count);
        void RemoveFromCart(int productId);
        void ClearCart();
        int GetCartCount();
        decimal GetCartTotal();
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}

