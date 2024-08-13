using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using System.Security.Claims;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewModel ShoppingCartVM { get; set; }
        public int MyProperty { get; set; }
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
                ShoppingCarts = _unitOfWork.ShoppingCart.GetAll(S => S.AppUserId == claim.Value , Includes: "Product")
            };

            foreach (var item in ShoppingCartVM.ShoppingCarts)
            {
                ShoppingCartVM.TotalPrice +=  (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
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
            var shppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(S => S.Id == cartId);

            if (shppingcart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shppingcart);
                _unitOfWork.Complete();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCart(shppingcart, 1);
            }
            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }


        public IActionResult Remove(int cartId)
        {
            var shoppingcart = _unitOfWork.ShoppingCart.GetFirstOrDefault(S => S.Id == cartId);

            _unitOfWork.ShoppingCart.Remove(shoppingcart);
            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }
    }
}
