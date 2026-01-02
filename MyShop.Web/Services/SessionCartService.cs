using Microsoft.AspNetCore.Http;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Utilities;
using System.Text.Json;

namespace MyShop.Web.Services
{
    public class SessionCartService : ISessionCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "ShoppingCart";

        public SessionCartService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        private List<CartItem> GetCartItemsFromSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new List<CartItem>();

            var cartJson = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCartItemsToSession(List<CartItem> cartItems)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var cartJson = JsonSerializer.Serialize(cartItems);
            session.SetString(CartSessionKey, cartJson);
            session.SetInt32(SD.SessionKey, cartItems.Count);
        }

        public List<CartItem> GetCartItems()
        {
            return GetCartItemsFromSession();
        }

        public void AddToCart(int productId, int count)
        {
            var cartItems = GetCartItemsFromSession();
            var existingItem = cartItems.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Count += count;
            }
            else
            {
                cartItems.Add(new CartItem { ProductId = productId, Count = count });
            }

            SaveCartItemsToSession(cartItems);
        }

        public void UpdateCartItem(int productId, int count)
        {
            var cartItems = GetCartItemsFromSession();
            var existingItem = cartItems.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                if (count <= 0)
                {
                    cartItems.Remove(existingItem);
                }
                else
                {
                    existingItem.Count = count;
                }
            }

            SaveCartItemsToSession(cartItems);
        }

        public void RemoveFromCart(int productId)
        {
            var cartItems = GetCartItemsFromSession();
            var itemToRemove = cartItems.FirstOrDefault(x => x.ProductId == productId);
            
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItemsToSession(cartItems);
            }
        }

        public void ClearCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(CartSessionKey);
                session.SetInt32(SD.SessionKey, 0);
            }
        }

        public int GetCartCount()
        {
            return GetCartItemsFromSession().Count;
        }

        public decimal GetCartTotal()
        {
            var cartItems = GetCartItemsFromSession();
            decimal total = 0;

            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    var price = product.PriceAfterDiscount > 0 ? product.PriceAfterDiscount : product.Price;
                    total += price * item.Count;
                }
            }

            return total;
        }
    }
}

