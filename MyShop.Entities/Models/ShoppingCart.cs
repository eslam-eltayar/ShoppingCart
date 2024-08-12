using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyShop.Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class ShoppingCart : BaseEntity
    {
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public int Count { get; set; }

        public string AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        [ValidateNever]
        public AppUser AppUser { get; set; }
    }
}
