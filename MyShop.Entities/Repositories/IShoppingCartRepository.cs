using MyShop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Repositories
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        int IncreaseCart(ShoppingCart cart, int count);
        int DecreaseCart(ShoppingCart cart, int count);
    }
}
