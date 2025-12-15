using System;
using System.ComponentModel.DataAnnotations;

namespace SUPFLY.Models
{
    public class BookingSearchModel
    {
        [Required(ErrorMessage = "Origin is required.")]
        [Display(Name = "Flying From (IATA Code)")] // <-- Updated Display Name
        public string Origin { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        [Display(Name = "Flying To (IATA Code)")] // <-- Updated Display Name
        public string Destination { get; set; }

        [Required(ErrorMessage = "Departure Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; } = DateTime.Today;
    }
}