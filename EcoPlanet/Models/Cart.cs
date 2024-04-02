using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    // Represents a user's shopping cart.
    public class Cart
    {
        [Key]
        public int cartId { get; set; }
        public string userId { get; set; }
        public virtual List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal totalPrice => Items.Sum(item => item.totalPrice);
    }

    // Represents an item within the shopping cart.
    public class CartItem
    {
        [Key]
        public int cartItemId { get; set; }
        public int goodsId { get; set; }
        public string goodsName { get; set; }
        public int goodsQuantity { get; set; }
        public decimal goodsPrice { get; set; }
        public string goodsImage { get; set; }
        public decimal totalPrice => goodsQuantity * goodsPrice;
        public virtual Cart Cart { get; set; }
    }

}
