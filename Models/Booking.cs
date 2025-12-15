using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SUPFLY.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // Relationship to Flight
        [Required]
        public int FlightId { get; set; }
        [ValidateNever]
        public Flight Flight { get; set; }

        // --- Relationship to Passenger (THIS MUST BE CORRECT) ---
        [Required]
        public int PassengerId { get; set; } // <--- Requires Passenger model to exist
        [ValidateNever]
        public Passenger Passenger { get; set; } // <--- Requires Passenger model to exist
        // --------------------------------------------------------

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(10)]
        [Display(Name = "Seat Number")]
        public string SeatNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Price Paid")]
        public decimal PricePaid { get; set; }
    }
}