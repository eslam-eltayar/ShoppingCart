using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Complete();

                TempData["Create"] = "Item has Created Successfuly";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var categoryInDb = _unitOfWork.Category.GetFirstOrDefault(X => X.Id == id);

            return View(categoryInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Complete();

                TempData["Update"] = "Item has Updated Successfuly";

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var categoryInDb = _unitOfWork.Category.GetFirstOrDefault(X => X.Id == id);

            return View(categoryInDb);
        }


        [HttpPost]
        public IActionResult ConfirmDelete(int? id)
        {
            var categoryInDb = _unitOfWork.Category.GetFirstOrDefault(X => X.Id == id);

            if (categoryInDb is null)
                return NotFound();

            _unitOfWork.Category.Remove(categoryInDb);
            _unitOfWork.Complete();

            TempData["Delete"] = "Item has Deleted Successfuly";

            return RedirectToAction(nameof(Index));

        }

    }
}
