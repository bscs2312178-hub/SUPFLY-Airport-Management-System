using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SUPFLY.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // --- OUTBOUND FLIGHT (GOING) ---
        [Required]
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        // --- PASSENGER INFO ---
        [Required]
        public int PassengerId { get; set; }
        public Passenger? Passenger { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        // --- BOOKING DETAILS ---
        public string? Status { get; set; } = "Confirmed"; // Confirmed, Cancelled
        public string? SeatNumber { get; set; } // Outbound Seat
        public string? ReturnSeatNumber { get; set; } // NEW: Seat for the return flight

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePaid { get; set; } // Total price for the whole journey

        // --- ROUND TRIP LOGIC ---
        public bool IsRoundTrip { get; set; } // True = Two Way, False = One Way

        public int? ReturnFlightId { get; set; } // The ID of the flight coming back

        [ForeignKey("ReturnFlightId")]
        public Flight? ReturnFlight { get; set; } // The actual return flight data
    }
}