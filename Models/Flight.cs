using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SUPFLY.Models
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        public string FlightNumber { get; set; }

        [Required]
        public int AircraftId { get; set; }

        [ValidateNever]
        public Aircraft Aircraft { get; set; }

        [Required]
        public int FromAirportId { get; set; }

        [ValidateNever]
        [ForeignKey("FromAirportId")]
        public Airport FromAirport { get; set; }

        [Required]
        public int ToAirportId { get; set; }

        [ValidateNever]
        [ForeignKey("ToAirportId")]
        public Airport ToAirport { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }
      

        [Required]
        [Display(Name = "Price")]
        [Column(TypeName = "decimal(18, 2)")] // Ensure correct mapping for currency
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
    }
}
