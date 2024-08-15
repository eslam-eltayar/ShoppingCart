using MyShop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.ViewModels
{
    public class ShoppingCartViewModel
    {
        public IEnumerable<ShoppingCart> ShoppingCarts { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderHeader OrderHeader { get; set; }

    }
}
