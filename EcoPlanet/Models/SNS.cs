using System;
using System.ComponentModel.DataAnnotations;
namespace EcoPlanet.Models
{
    public class SNS
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
