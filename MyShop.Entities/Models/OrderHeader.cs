using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.Models
{
    public class OrderHeader : BaseEntity
    {
        [ValidateNever]
        public AppUser AppUser { get; set; } // Nav prop
        public string AppUserId { get; set; } // FK

        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }

        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public DateTime PaymentDate { get; set; }


        // Stripe 

        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }


        // Data of User
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^(010|011|012|015)\d{8}$", ErrorMessage = "Please enter a valid Egyptian phone number.")]
        public string Phone { get; set; }

    }
}
