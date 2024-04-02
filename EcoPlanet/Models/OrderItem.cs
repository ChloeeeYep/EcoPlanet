using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int goodsId { get; set; }

        public string goodsName { get; set; }

        public int goodsQuantity { get; set; }

        public decimal goodsPrice { get; set; }

        public decimal TotalPrice => goodsQuantity * goodsPrice;

        public string SellerId { get; set; }

        public virtual Order Order { get; set; }
    }
}
