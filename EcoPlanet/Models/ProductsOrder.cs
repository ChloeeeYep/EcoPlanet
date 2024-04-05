using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{

    public class ProductsOrder
    {

        [Key]
        public int ProductsOrderId { get; set; }

        public string Email { get; set; }

        public DateTime ProductsOrderDate { get; set; }

        public decimal TotalPrice => ProductsOrderItems.Sum(item => item.TotalPrice);

        public string Contact { get; set; }

        public string Address { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public string OrderStatus { get; set; }

        public string DriverId { get; set; }

        public virtual List<ProductsOrderItem> ProductsOrderItems { get; set; } = new List<ProductsOrderItem>();

    }
}
