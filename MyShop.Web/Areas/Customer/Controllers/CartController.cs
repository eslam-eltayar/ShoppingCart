using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using MyShop.Web.Services;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionCartService _cartService;

        public CartController(IUnitOfWork unitOfWork, ISessionCartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cartItems = _cartService.GetCartItems();
            var viewModel = new ShoppingCartViewModel();

            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == item.ProductId, Includes: "Category");
                if (product != null)
                {
                    viewModel.CartItems.Add(new CartItemViewModel
                    {
                        ProductId = item.ProductId,
                        Product = product,
                        Count = item.Count
                    });

                    var price = product.PriceAfterDiscount > 0 ? product.PriceAfterDiscount : product.Price;
                    viewModel.TotalPrice += price * item.Count;
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var cartItems = _cartService.GetCartItems();
            
            if (cartItems.Count == 0)
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            var viewModel = new ShoppingCartViewModel
            {
                OrderHeader = new OrderHeader()
            };

            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == item.ProductId, Includes: "Category");
                if (product != null)
                {
                    viewModel.CartItems.Add(new CartItemViewModel
                    {
                        ProductId = item.ProductId,
                        Product = product,
                        Count = item.Count
                    });

                    var price = product.PriceAfterDiscount > 0 ? product.PriceAfterDiscount : product.Price;
                    viewModel.TotalPrice += price * item.Count;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult POSTSummary(ShoppingCartViewModel shoppingCartVM)
        {
            var cartItems = _cartService.GetCartItems();
            
            if (cartItems.Count == 0)
            {
                TempData["error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // Calculate total price
            decimal totalPrice = 0;
            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    var price = product.PriceAfterDiscount > 0 ? product.PriceAfterDiscount : product.Price;
                    totalPrice += price * item.Count;
                }
            }

            // Create OrderHeader
            var orderHeader = new OrderHeader
            {
                FullName = shoppingCartVM.OrderHeader.FullName,
                PhoneNumber = shoppingCartVM.OrderHeader.PhoneNumber,
                Address = shoppingCartVM.OrderHeader.Address,
                Notes = shoppingCartVM.OrderHeader.Notes,
                OrderDate = DateTime.Now,
                OrderStatus = SD.New,
                TotalPrice = totalPrice
            };

            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Complete();

            // Create OrderDetails
            foreach (var item in cartItems)
            {
                var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    var price = product.PriceAfterDiscount > 0 ? product.PriceAfterDiscount : product.Price;
                    
                    var orderDetail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        OrderHeaderId = orderHeader.Id,
                        Price = price,
                        Count = item.Count
                    };

                    _unitOfWork.OrderDetail.Add(orderDetail);
                }
            }

            _unitOfWork.Complete();

            // Clear cart
            _cartService.ClearCart();

            return RedirectToAction("OrderConfirmation", new { id = orderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            
            if (orderHeader == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }

            return View(orderHeader);
        }

        public IActionResult Plus(int productId)
        {
            var cartItems = _cartService.GetCartItems();
            var item = cartItems.FirstOrDefault(x => x.ProductId == productId);
            
            if (item != null)
            {
                _cartService.UpdateCartItem(productId, item.Count + 1);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Minus(int productId)
        {
            var cartItems = _cartService.GetCartItems();
            var item = cartItems.FirstOrDefault(x => x.ProductId == productId);
            
            if (item != null)
            {
                if (item.Count <= 1)
                {
                    _cartService.RemoveFromCart(productId);
                }
                else
                {
                    _cartService.UpdateCartItem(productId, item.Count - 1);
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToAction("Index");
        }
    }
}
