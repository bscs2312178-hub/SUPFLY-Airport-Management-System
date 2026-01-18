using System.ComponentModel.DataAnnotations;
using SUPFLY.Models; // This lets the file see your Flight model

namespace SUPFLY.ViewModels
{
    public class FlightSearchViewModel
    {
        [Required(ErrorMessage = "Origin is required")]
        public int FromAirportId { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        public int ToAirportId { get; set; }

        [Required(ErrorMessage = "Please select a departure date")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; } = DateTime.Today;

        // --- NEW THINGS FOR TWO-WAY TICKETS ---

        public bool IsRoundTrip { get; set; } // Switch: True = Round Trip, False = One Way

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; } // The "?" means it can be empty if it's a One-Way

        // --- THESE HOLD THE SEARCH RESULTS ---

        public List<Flight> OutboundFlights { get; set; } = new List<Flight>(); // List of going flights
        public List<Flight> ReturnFlights { get; set; } = new List<Flight>();  // List of coming back flights
    }
}