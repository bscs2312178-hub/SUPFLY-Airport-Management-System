using System.ComponentModel.DataAnnotations;

namespace SUPFLY.ViewModels
{
    public class FlightSearchViewModel
    {
        [Required(ErrorMessage = "Origin is required")]
        public string FromCode { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        public string ToCode { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; } = DateTime.Today;
    }
}