using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SUPFLY.Areas.Identity.Data;
using SUPFLY.Data;
using SUPFLY.Models;
using SUPFLY.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SUPFLY.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<SUPFLYUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<SUPFLYUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Bookings/Dashboard
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalFlights = await _context.Flights.CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                TotalPassengers = await _context.Passengers.CountAsync(),
                TotalRevenue = await _context.Bookings
                    .Where(b => b.Status != "Cancelled")
                    .SumAsync(b => b.PricePaid),

                RecentBookings = await _context.Bookings
                    .Include(b => b.Passenger)
                    .Include(b => b.Flight)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .Select(b => new RecentBookingViewModel
                    {
                        BookingId = b.Id,
                        PassengerName = b.Passenger.FirstName + " " + b.Passenger.LastName,
                        FlightNumber = b.Flight.FlightNumber,
                        Amount = b.PricePaid,
                        Date = b.BookingDate
                    }).ToListAsync()
            };

            return View(stats);
        }

        // GET: Bookings (Admin View)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight).ThenInclude(f => f.ToAirport)
                .Include(b => b.Passenger);
            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight).ThenInclude(f => f.ToAirport)
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (booking.Passenger?.UserId != currentUserId) return Forbid();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create(int? flightId)
        {
            var userId = _userManager.GetUserId(User);
            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (passenger == null)
            {
                var user = await _userManager.GetUserAsync(User);
                passenger = new Passenger
                {
                    UserId = userId,
                    FirstName = "New",
                    LastName = user?.Email ?? "Passenger",
                    Email = user?.Email ?? ""
                };
                _context.Passengers.Add(passenger);
                await _context.SaveChangesAsync();
            }

            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", flightId);
            ViewData["PassengerId"] = passenger.Id;

            ViewBag.TakenSeats = await _context.Bookings
                .Where(b => b.FlightId == flightId && b.Status != "Cancelled")
                .Select(b => b.SeatNumber)
                .ToListAsync();

            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FlightId,PassengerId,SeatNumber")] Booking booking)
        {
            // 1. Tell the controller to ignore validation for fields we set manually
            ModelState.Remove("Status");
            ModelState.Remove("PricePaid");
            ModelState.Remove("BookingDate");
            ModelState.Remove("Passenger");
            ModelState.Remove("Flight");

            var flight = await _context.Flights.FindAsync(booking.FlightId);

            if (flight != null)
            {
                // Check if seat is already taken
                bool isSeatTaken = await _context.Bookings
                    .AnyAsync(b => b.FlightId == booking.FlightId &&
                                   b.SeatNumber == booking.SeatNumber &&
                                   b.Status != "Cancelled");

                if (isSeatTaken)
                {
                    ModelState.AddModelError("SeatNumber", $"Seat {booking.SeatNumber} is already reserved. Please choose another.");
                }

                // Set values that aren't in the form
                booking.PricePaid = flight.Price;
                booking.Status = "Confirmed";
                booking.BookingDate = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                // Redirecting to MyBookings after successful save
                return RedirectToAction(nameof(MyBookings));
            }

            // If we are here, something went wrong (Validation failed)
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);

            // Re-fetch taken seats so the UI stays consistent
            ViewBag.TakenSeats = await _context.Bookings
                .Where(b => b.FlightId == booking.FlightId && b.Status != "Cancelled")
                .Select(b => b.SeatNumber)
                .ToListAsync();

            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Email", booking.PassengerId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightId,PassengerId,BookingDate,SeatNumber,PricePaid,Status")] Booking booking)
        {
            if (id != booking.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null) _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (passenger == null)
            {
                ViewBag.NoPassengerProfile = true;
                return View("MyBookings", new List<Booking>());
            }

            var userBookings = await _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight).ThenInclude(f => f.ToAirport)
                .Where(b => b.PassengerId == passenger.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(userBookings);
        }

        private bool BookingExists(int id) => _context.Bookings.Any(e => e.Id == id);
    }
}