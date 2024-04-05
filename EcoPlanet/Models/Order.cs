using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string Email { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice => OrderItems.Sum(item => item.TotalPrice);

        public string Contact { get; set; }

        public string Address { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public string OrderStatus { get; set; }

        public string DriverId { get; set; }

        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();


    }
}
