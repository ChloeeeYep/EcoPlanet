using System.ComponentModel.DataAnnotations;
using EcoPlanet.Models;
using System.Collections.Generic;

namespace EcoPlanet.ViewModels
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public Dictionary<string, string> ImageUrls { get; set; }

        public OrderViewModel()
        {
            Order = new Order(); // Initializes the Order property
            ImageUrls = new Dictionary<string, string>(); // Initializes the ImageUrls property
        }
    }

}
