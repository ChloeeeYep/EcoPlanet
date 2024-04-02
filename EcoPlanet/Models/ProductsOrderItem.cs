using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class ProductsOrderItem
    {
        [Key]
        public int ProductsOrderItemId { get; set; }

        public int productsId { get; set; }

        public string productsName { get; set; }

        public int productsQuantity { get; set; }

        public decimal productsPrice { get; set; }

        public decimal TotalPrice => productsQuantity * productsPrice;

        public virtual ProductsOrder ProductsOrder { get; set; }
    }
}
