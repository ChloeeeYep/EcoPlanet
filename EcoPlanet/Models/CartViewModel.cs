using Amazon.S3.Model;
using EcoPlanet.Models;
using System.Collections.Generic;

namespace EcoPlanet.ViewModels
{
    public class CartViewModel
    {
        public Cart Cart { get; set; }
        public Dictionary<string, string> ImageUrls { get; set; } 

        public Dictionary<int, int> MaxQuantities { get; set; }

        public CartViewModel()
        {
            Cart = new Cart(); // Initializes the Cart property
            ImageUrls = new Dictionary<string, string>(); // Initializes the ImageUrls property
            MaxQuantities = new Dictionary<int, int>();
        }

    }
}
