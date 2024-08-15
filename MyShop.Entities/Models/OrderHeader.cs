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

        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        [Phone]
        public string Phone { get; set; }

    }
}
