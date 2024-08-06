using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [DisplayName("Image")]
        [ValidateNever]
        public string? Img { get; set; }
        public decimal Price { get; set; }

        [DisplayName("Category")]
        [ValidateNever]
        public int CategoryId { get; set; } // FK

        [ValidateNever]
        public Category Category { get; set; } // Navigational Prop

    }
}
