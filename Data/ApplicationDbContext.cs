using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SUPFLY.Areas.Identity.Data;
using SUPFLY.Models;

namespace SUPFLY.Data
{
    public class ApplicationDbContext : IdentityDbContext<SUPFLYUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Airport> Airports { get; set; }
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fix for the Decimal warning
            modelBuilder.Entity<Booking>()
                .Property(b => b.PricePaid)
                .HasPrecision(18, 2); // 18 digits total, 2 after the decimal point

            modelBuilder.Entity<Flight>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);
            // 1. ALWAYS call base first for Identity
            base.OnModelCreating(modelBuilder);

            // 2. Fix for Multiple Foreign Keys (Airport Restriction)
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.FromAirport)
                .WithMany()
                .HasForeignKey(f => f.FromAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ToAirport)
                .WithMany()
                .HasForeignKey(f => f.ToAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Unique Seat Constraint
            // Prevents the same seat being booked twice on the same flight
            modelBuilder.Entity<Booking>()
        .HasIndex(b => new { b.FlightId, b.SeatNumber })
        .IsUnique();
        }
    }
}