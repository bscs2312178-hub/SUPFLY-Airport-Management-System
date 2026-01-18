using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SUPFLY.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace SUPFLY.Models
{
    public class Passenger
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Passport Number")]
        [StringLength(20)]
        public string PassportNumber { get; set; }

        // Link back to the Identity User that created this Passenger profile
        // *** FIX: Changed to nullable string? to resolve FK conflict on existing data ***
        [Required]
        public string? UserId { get; set; }

        [ValidateNever]
        [ForeignKey("UserId")]
        public SUPFLYUser? User { get; set; } // Also make the navigation property nullable

        // Collection of bookings for easy look-up
        [ValidateNever]
        public ICollection<Booking> Bookings { get; set; }
    }
}