namespace EcoPlanet.Models
{
    public class CheckoutViewModel
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string CardHolderName { get; set; }
        public string CardNumber { get; set; } 
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}
