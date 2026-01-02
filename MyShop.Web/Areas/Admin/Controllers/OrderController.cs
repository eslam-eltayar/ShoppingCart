using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderViewModel OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(bool? filterToday)
        {
            ViewBag.FilterToday = filterToday ?? false;
            return View();
        }

        public IActionResult GetData(bool? filterToday)
        {
            IEnumerable<OrderHeader> orderHeaders;
            
            if (filterToday == true)
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }
            else
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll()
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
            }
            
            return Json(new { data = orderHeaders });
        }

        public IActionResult Details(int orderid)
        {
            OrderViewModel order = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == orderid),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(o => o.OrderHeaderId == orderid, Includes: "Product")
            };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            if (orderFromDb == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            orderFromDb.FullName = OrderVM.OrderHeader.FullName;
            orderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderFromDb.Address = OrderVM.OrderHeader.Address;
            orderFromDb.Notes = OrderVM.OrderHeader.Notes;

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

            TempData["success"] = "Order details updated successfully.";

            return RedirectToAction(nameof(Details), "Order", new { orderid = orderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(string status)
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            if (orderFromDb == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            if (status == SD.Processing)
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Processing);
            }
            else if (status == SD.Completed)
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Completed);
            }
            else if (status == SD.Cancelled)
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Cancelled);
            }

            _unitOfWork.Complete();

            TempData["success"] = "Order status updated successfully.";

            return RedirectToAction(nameof(Details), "Order", new { orderid = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShipping()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            if (orderFromDb == null)
            {
                TempData["error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                TempData["error"] = "Please enter a carrier.";
                return RedirectToAction(nameof(Details), "Order", new { orderid = OrderVM.OrderHeader.Id });
            }

            if (string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                TempData["error"] = "Please enter a tracking number.";
                return RedirectToAction(nameof(Details), "Order", new { orderid = OrderVM.OrderHeader.Id });
            }

            orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderFromDb.OrderStatus = SD.Completed;
            orderFromDb.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Complete();

            TempData["success"] = "Order has been shipped successfully.";

            return RedirectToAction(nameof(Details), "Order", new { orderid = OrderVM.OrderHeader.Id });
        }
    }
}
