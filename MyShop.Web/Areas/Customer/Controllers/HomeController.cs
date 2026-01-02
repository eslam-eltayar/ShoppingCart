using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Repositories;
using MyShop.Entities.Models;
using MyShop.Entities.ViewModels;
using X.PagedList.Extensions;
using MyShop.Web.Services;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionCartService _cartService;

        public HomeController(IUnitOfWork unitOfWork, ISessionCartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public IActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 8;

            ViewBag.CurrentPage = pageNumber;

            var products = _unitOfWork.Product.GetAll().ToPagedList(pageNumber, pageSize);

            return View(products);
        }

        public IActionResult Details(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["error"] = "Invalid product ID.";
                return RedirectToAction("Index");
            }

            int productId = id.Value;
            var product = _unitOfWork.Product.GetFirstOrDefault(v => v.Id == productId, Includes: "Category");
            
            if (product == null)
            {
                TempData["error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            var cartItem = _cartService.GetCartItems().FirstOrDefault(x => x.ProductId == productId);
            var count = cartItem?.Count ?? 1;

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Count = count
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int count, int? page)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            if (count <= 0)
            {
                count = 1;
            }

            try
            {
                _cartService.AddToCart(productId, count);
                TempData["success"] = $"{product.Name} added to cart successfully!";
            }
            catch (Exception)
            {
                TempData["error"] = "An error occurred while adding to cart. Please try again.";
            }

            // If page parameter exists, user came from Index page - redirect back to Index
            if (page.HasValue)
            {
                return RedirectToAction("Index", new { page = page.Value });
            }

            // Otherwise, redirect to Index (default behavior)
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(int productId, int count)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            if (count <= 0)
            {
                count = 1;
            }

            try
            {
                _cartService.AddToCart(productId, count);
                TempData["success"] = $"{product.Name} added to cart successfully!";
            }
            catch (Exception)
            {
                TempData["error"] = "An error occurred while adding to cart. Please try again.";
            }

            // Stay on Details page
            return RedirectToAction("Details", new { id = productId });
        }
    }
}
