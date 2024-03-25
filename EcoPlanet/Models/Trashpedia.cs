using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class Trashpedia
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Materials { get; set; }

        public string? Actions { get; set; }

        public string? Alternatives { get; set; }

        public string? Images { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
