using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using NuGet.Packaging.Signing;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetData()
        {
            var products = _unitOfWork.Product.GetAll(Includes: "Category");
            return Json(new { data = products });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductViewModel productVm = new ProductViewModel()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(C => new SelectListItem
                {
                    Text = C.Name,
                    Value = C.Id.ToString()

                })
            };

            return View(productVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductViewModel productVm, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _webHostEnvironment.WebRootPath; // path of WWWroot
                string uploadPath = Path.Combine(rootPath, "Images", "Products");

                // Ensure the directory exists
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                if (file != null && file.Length > 0)
                {
                    // Generate a unique file name
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(file.FileName); // Get the file extension from the uploaded file
                    string filePath = Path.Combine(uploadPath, fileName + extension);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Store the relative path to the image
                    productVm.Product.Img = Path.Combine("Images", "Products", fileName + extension);
                }

                // Set Price to PriceAfterDiscount (selling price)
                // If PriceAfterDiscount is 0, use PriceBeforeDiscount or Price as fallback
                if (productVm.Product.PriceAfterDiscount > 0)
                {
                    productVm.Product.Price = productVm.Product.PriceAfterDiscount;
                }
                else if (productVm.Product.PriceBeforeDiscount.HasValue && productVm.Product.PriceBeforeDiscount.Value > 0)
                {
                    productVm.Product.Price = productVm.Product.PriceBeforeDiscount.Value;
                    productVm.Product.PriceAfterDiscount = productVm.Product.PriceBeforeDiscount.Value;
                }
                // If both are 0, Price should already have a value from the form

                _unitOfWork.Product.Add(productVm.Product);
                _unitOfWork.Complete();

                TempData["Create"] = "Item has Created Successfuly";

                return RedirectToAction(nameof(Index));
            }

            return View(productVm.Product);
        }


        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var product = _unitOfWork.Product.GetFirstOrDefault(P => P.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            // Initialize PriceAfterDiscount from Price if not set (for existing products)
            if (product.PriceAfterDiscount == 0 && product.Price > 0)
            {
                product.PriceAfterDiscount = product.Price;
            }

            ProductViewModel productVm = new ProductViewModel()
            {
                Product = product,
                CategoryList = _unitOfWork.Category.GetAll().Select(C => new SelectListItem
                {
                    Text = C.Name,
                    Value = C.Id.ToString()

                })
            };

            return View(productVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductViewModel productVm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _webHostEnvironment.WebRootPath; // path of WWWroot
                string uploadPath = Path.Combine(rootPath, "Images", "Products");

                // Ensure the directory exists
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                if (file != null && file.Length > 0)
                {
                    // Generate a unique file name
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(file.FileName); // Get the file extension from the uploaded file
                    string filePath = Path.Combine(uploadPath, fileName + extension);

                    if (productVm.Product.Img != null)
                    {
                        var oldimg = Path.Combine(rootPath, productVm.Product.Img.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimg))
                        {
                            System.IO.File.Delete(oldimg);
                        }
                    }


                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Store the relative path to the image
                    productVm.Product.Img = Path.Combine("Images", "Products", fileName + extension);
                }

                // Set Price to PriceAfterDiscount (selling price)
                // If PriceAfterDiscount is 0, keep the existing Price value
                if (productVm.Product.PriceAfterDiscount > 0)
                {
                    productVm.Product.Price = productVm.Product.PriceAfterDiscount;
                }
                else
                {
                    // Get existing product to preserve Price if PriceAfterDiscount is not set
                    var existingProduct = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == productVm.Product.Id);
                    if (existingProduct != null && existingProduct.Price > 0)
                    {
                        productVm.Product.Price = existingProduct.Price;
                        productVm.Product.PriceAfterDiscount = existingProduct.Price;
                    }
                }

                _unitOfWork.Product.Update(productVm.Product);
                _unitOfWork.Complete();

                TempData["Update"] = "Item has Updated Successfuly";

                return RedirectToAction(nameof(Index));
            }

            return View(productVm.Product);
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productInDb = _unitOfWork.Product.GetFirstOrDefault(X => X.Id == id);

            if (productInDb is null)
                return Json(new { success = false, message = "Error while Deleting" });

            _unitOfWork.Product.Remove(productInDb);

            var oldImg = Path.Combine(_webHostEnvironment.WebRootPath, productInDb.Img.TrimStart('\\'));

            if (System.IO.File.Exists(oldImg))
                System.IO.File.Delete(oldImg);


            _unitOfWork.Complete();
            return Json(new { success = true, message = "Product has been Deleted Successfuly" });
        }
    }
}