using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Repositories;
using MyShop.Entities.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MyShop.DataAccess.Repositories.Imp;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll();

            return View(products);
        }

        public IActionResult Details(int Id)
        {
            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = Id,
                Product = _unitOfWork.Product.GetFirstOrDefault(v => v.Id == Id, Includes: "Category"),
                Count = 1
            };
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details([FromForm] ShoppingCart shoppingCart)
        {
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.AppUserId = claim.Value;

            ShoppingCart cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                C => C.AppUserId == claim.Value && C.ProductId == shoppingCart.ProductId
                );


            if (cartObj is null)
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            else
                _unitOfWork.ShoppingCart.IncreaseCart(cartObj, shoppingCart.Count);


            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }
    }
}
