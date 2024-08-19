using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewModel ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(S => S.AppUserId == claim.Value, Includes: "Product")
            };

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                ShoppingCartVM.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartViewModel()
            {
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == claim.Value, Includes: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.AppUser = _unitOfWork.AppUser.GetFirstOrDefault(x => x.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.AppUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.AppUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.AppUser.City;
            ShoppingCartVM.OrderHeader.Phone = ShoppingCartVM.OrderHeader.AppUser.PhoneNumber;

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);

        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult POSTSummary(ShoppingCartViewModel shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // get current user

            shoppingCartVM.ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == claim.Value, Includes: "Product");

            shoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.AppUserId = claim.Value;


            foreach (var item in shoppingCartVM.ShoppingCarts)
            {
                shoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Complete();

            foreach (var item in shoppingCartVM.ShoppingCarts)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Complete();
            }

            var domain = "https://localhost:7032/";

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/orderconfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            foreach (var item in shoppingCartVM.ShoppingCarts)
            {
                var sessionLineOption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineOption);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            shoppingCartVM.OrderHeader.SessionId = session.Id;



            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);


            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(id, SD.Approve, SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _unitOfWork.Complete();
            }

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == orderHeader.AppUserId).ToList();
            // get oder cart of current user 

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts); // remove the cart of user after order confirmation
            _unitOfWork.Complete();

            var count = _unitOfWork.ShoppingCart.GetAll(s => s.AppUserId == orderHeader.AppUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return View(id);
        }



        public IActionResult Plus(int cartId)
        {
            var shppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(S => S.Id == cartId);
            _unitOfWork.ShoppingCart.IncreaseCart(shppingcart, 1);

            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }


        public IActionResult Minus(int cartId)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(S => S.Id == cartId);

            if (shoppingcart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingcart);

                var count = _unitOfWork.ShoppingCart.GetAll(s => s.AppUserId == shoppingcart.AppUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.SessionKey, count--);

            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCart(shoppingcart, 1);
            }
            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(S => S.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(shoppingcart);
            _unitOfWork.Complete();

            var count = _unitOfWork.ShoppingCart.GetAll(s => s.AppUserId == shoppingcart.AppUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return RedirectToAction("Index");
        }

    }
}
