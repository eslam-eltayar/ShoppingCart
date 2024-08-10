using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyShop.Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class ShoppingCart : BaseEntity
    {
        public int ProductId { get; set; } // FK

        [ValidateNever]  // should not be validated during model binding
        public Product Product { get; set; }

        [Range(1, 100, ErrorMessage = "You must enter value between 1 to 100")]
        public int Count { get; set; }

        public string AppUserId { get; set; } // FK

        [ValidateNever]
        public AppUser AppUser { get; set; }
    }
}
