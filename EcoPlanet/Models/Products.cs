using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class Products
    {
        [Key]
        public int productsId { get; set; }

        public string ? productsType { get; set; }

        public string? productsName { get; set; }

        public string? productsDescriptions { get; set;}

        public decimal productsPrice { get; set; }

        public int productsQuantity { get; set;}

        public string ? productsStatus { get; set; }

        public string ? productsImage { get; set; }


        public string ? adminId { get; set; }


        public DateTime createdAt { get; set; }


    }
}
