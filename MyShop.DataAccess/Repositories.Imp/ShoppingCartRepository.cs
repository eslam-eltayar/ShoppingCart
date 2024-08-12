using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.Repositories.Imp
{
    public class ShoppingCartRepository : GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }

        public int DecreaseCart(ShoppingCart cart, int count)
        {
            cart.Count -= count;
            return cart.Count;
        }

        public int IncreaseCart(ShoppingCart cart, int count)
        {
            cart.Count += count;
            return cart.Count;
        }
    }
}
