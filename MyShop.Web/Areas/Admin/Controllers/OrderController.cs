using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty] // bind data from HTTP requests (such as FORM data, query strings, route data, or JSON)
        public OrderViewModel OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = _unitOfWork.OrderHeader.GetAll(Includes: "AppUser");
            return Json(new { data = orderHeaders });
        }

        public IActionResult Details(int orderid)
        {
            OrderViewModel order = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == orderid, Includes: "AppUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(o => o.OrderHeaderId == orderid, Includes: "Product")

            };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            orderFromDb.Name = OrderVM.OrderHeader.Name;
            orderFromDb.Phone = OrderVM.OrderHeader.Phone;
            orderFromDb.Address = OrderVM.OrderHeader.Address;
            orderFromDb.City = OrderVM.OrderHeader.City;

            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Complete();

            TempData["Update"] = "Item has Updated Successfuly";

            return RedirectToAction(nameof(Details), "Order", new { orderid = orderFromDb.Id });
        }
    }
}
