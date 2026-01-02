using MyShop.Entities.Models;

namespace MyShop.Entities.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public int Count { get; set; } = 1;
    }
}

