using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Utilities;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private IUnitOfWork _unitOfWork;
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // New Orders (Today)
            ViewBag.NewOrdersToday = _unitOfWork.OrderHeader
                .GetAll(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .Count();

            // Total Orders
            ViewBag.TotalOrders = _unitOfWork.OrderHeader.GetAll().Count();

            // Products
            ViewBag.Products = _unitOfWork.Product.GetAll().Count();

            // Latest 5 Orders
            ViewBag.LatestOrders = _unitOfWork.OrderHeader
                .GetAll()
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}
