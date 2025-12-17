namespace SUPFLY.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        public int PassengerId { get; set; }
        public Passenger? Passenger { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        // --- ADD THESE MISSING PROPERTIES ---
        public string? Status { get; set; } = "Confirmed"; // e.g., Confirmed, Cancelled
        public string? SeatNumber { get; set; }
        public decimal PricePaid { get; set; }
    }
}