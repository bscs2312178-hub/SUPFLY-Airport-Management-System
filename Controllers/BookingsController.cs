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
        public async Task<IActionResult> Create(int flightId, int? returnFlightId, bool isRoundTrip)
        {
            // 1. Find the Outbound Flight
            var outboundFlight = await _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(f => f.Id == flightId);

            if (outboundFlight == null) return NotFound();

            // 2. Find the Return Flight
            Flight? returnFlight = null;
            if (isRoundTrip && returnFlightId.HasValue)
            {
                returnFlight = await _context.Flights
                    .Include(f => f.FromAirport)
                    .Include(f => f.ToAirport)
                    .FirstOrDefaultAsync(f => f.Id == returnFlightId);
            }

            // --- STEP 2.5: FIND THE LOGGED-IN PASSENGER ---
            // This part is crucial to fix the Foreign Key error!
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (passenger == null)
            {
                // If the user hasn't created a passenger profile yet, 
                // they shouldn't be able to book.
                return RedirectToAction("Create", "Passengers");
            }

            // 3. Create the Booking object with the PassengerId PRE-FILLED
            var booking = new Booking
            {
                FlightId = flightId,
                ReturnFlightId = returnFlightId,
                IsRoundTrip = isRoundTrip,
                PassengerId = passenger.Id, // <--- This connects the booking to YOU
                PricePaid = outboundFlight.Price + (returnFlight?.Price ?? 0),
                Status = "Confirmed"
            };

            // 4. Send details to the View
            ViewBag.OutboundFlight = outboundFlight;
            ViewBag.ReturnFlight = returnFlight;
            ViewBag.PassengerId = passenger.Id; // Sent for the hidden field in the form

            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FlightId,PassengerId,PricePaid,IsRoundTrip,ReturnFlightId,SeatNumber,ReturnSeatNumber,Status")] Booking booking)
        {
            // If the page reloads, it's because this 'ModelState.IsValid' is FALSE
            if (ModelState.IsValid)
            {
                booking.BookingDate = DateTime.Now;
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = booking.Id });
            }

            // If we are here, something is invalid. 
            // Let's reload the view data so the page doesn't break.
            ViewBag.PassengerId = new SelectList(_context.Passengers, "Id", "Email", booking.PassengerId);
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
                // --- ADD THESE TWO LINES BELOW ---
                .Include(b => b.ReturnFlight).ThenInclude(f => f.FromAirport)
                .Include(b => b.ReturnFlight).ThenInclude(f => f.ToAirport)
                // ----------------------------------
                .Where(b => b.PassengerId == passenger.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(userBookings);
        }
        private List<SelectListItem> GetStatusOptions(string currentStatus)
        {
            var statuses = new List<string> { "Confirmed", "Checked-In", "Boarded", "Cancelled", "Completed" };
            return statuses.Select(s => new SelectListItem
            {
                Value = s,
                Text = s,
                Selected = s == currentStatus
            }).ToList();
        }
        // GET: Bookings/Manage
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var allBookings = await _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight).ThenInclude(f => f.ToAirport)
                .Include(b => b.Passenger)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            // --- CALCULATE REVENUE STATS ---
            ViewBag.TotalRevenue = allBookings.Where(b => b.Status != "Cancelled").Sum(b => b.PricePaid);
            ViewBag.TotalBookings = allBookings.Count;
            ViewBag.ActiveBookings = allBookings.Count(b => b.Status == "Confirmed" || b.Status == "Checked-In" || b.Status == "Boarded");
            ViewBag.CancelledBookings = allBookings.Count(b => b.Status == "Cancelled");

            return View(allBookings);
        }

        // POST: Bookings/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.Status = newStatus;
            await _context.SaveChangesAsync();

            // Redirect back to the management page
            return RedirectToAction(nameof(Manage));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(b => b.Id == id);

            // Security check: Only the owner can cancel, and only if it's just 'Confirmed'
            if (booking != null && booking.Passenger?.UserId == userId && booking.Status == "Confirmed")
            {
                booking.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your booking has been cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "This booking cannot be cancelled at this stage.";
            }

            return RedirectToAction(nameof(MyBookings));
        }

        private bool BookingExists(int id) => _context.Bookings.Any(e => e.Id == id);
    }
}