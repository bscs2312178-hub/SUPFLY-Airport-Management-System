using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SUPFLY.Areas.Identity.Data; // <-- Using your provided namespace
using SUPFLY.Models;
using System.Collections;

namespace SUPFLY.Data
{
    // The correct inheritance for Identity integration
    public class ApplicationDbContext : IdentityDbContext<SUPFLYUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your existing DbSet declarations for the Airline System
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public IEnumerable Aircraft { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: This call MUST come first to configure the Identity tables
            base.OnModelCreating(modelBuilder);

            // ⚠️ FIX FOR MULTIPLE FOREIGN KEYS (YOU ALREADY HAD THIS! NICE JOB!)
            // EF Core by default cannot determine which cascade path to use when you have 
            // two foreign keys (FromAirportId and ToAirportId) pointing to the same table (Airport).
            // We disable the automatic cascade delete behavior.

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.FromAirport)
                .WithMany()
                .HasForeignKey(f => f.FromAirportId)
                // This line prevents the potential database error 
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ToAirport)
                .WithMany()
                .HasForeignKey(f => f.ToAirportId)
                // This line prevents the potential database error
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}