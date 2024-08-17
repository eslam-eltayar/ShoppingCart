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
    public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader orderHeader)
        {
            _context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateOrderStatus(int id, string? orderStatus, string? paymentStatus)
        {
            var orderHeaderFromDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);

            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.OrderStatus = orderStatus;
                orderHeaderFromDb.OrderDate = DateTime.Now;

                if (paymentStatus != null)
                {
                    orderHeaderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }
    }
}
