using System.ComponentModel.DataAnnotations;

namespace EcoPlanet.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Correct { get; set; }

        public string? Wrong1 { get; set; }

        public string? Wrong2 { get; set; }

        public string? Wrong3 { get; set; }
    }
}
