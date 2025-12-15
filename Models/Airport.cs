using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SUPFLY.Models
{
    public class Airport
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // FIXED PROPERTY NAME:
        [Required]
        [StringLength(3)]
        [Display(Name = "Airport Code")]
        public string Code { get; set; }
        // ------------------------

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }
    }
}