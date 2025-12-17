namespace SUPFLY.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalFlights { get; set; }
        public int TotalBookings { get; set; }
        public int TotalPassengers { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<RecentBookingViewModel> RecentBookings { get; set; } = new();
    }

    public class RecentBookingViewModel
    {
        public int BookingId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}