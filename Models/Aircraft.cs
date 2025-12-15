using System.ComponentModel.DataAnnotations;

namespace SUPFLY.Models
{
    public class Aircraft
    {
        public int Id { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        [Required]
        public int Capacity { get; set; }
    }
}
