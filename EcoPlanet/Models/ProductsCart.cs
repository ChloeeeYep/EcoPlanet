using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class ProductsCart
    {
        [Key]
        public int productsCartId { get; set; }
        public string userId { get; set; }
        public virtual List<ProductsCartItem> Items { get; set; } = new List<ProductsCartItem>();
        public decimal totalPrice => Items.Sum(item => item.totalPrice);
    }

    public class ProductsCartItem
    {
        [Key]
        public int productsCartItemId { get; set; }
        public int productsId { get; set; }
        public string productsName { get; set; }
        public int productsQuantity { get; set; }
        public decimal productsPrice { get; set; }
        public string productsImage { get; set; }
        public decimal totalPrice => productsQuantity * productsPrice;
        public virtual ProductsCart ProductsCart { get; set; }

        public virtual Products Products { get; set; }
    }

}
