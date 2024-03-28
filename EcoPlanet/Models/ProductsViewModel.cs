
using Amazon.S3.Model;
using System.Collections.Generic;

namespace EcoPlanet.Models
{
    public class ProductsViewModel
    {
        public List<Products> ProductsList { get; set; }
        public List<S3Object> ImageList { get; set; }
    }
}
