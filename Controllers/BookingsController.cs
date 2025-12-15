using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SUPFLY.Data;
using SUPFLY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims; // ADDED for finding the current user ID

namespace SUPFLY.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ... (Existing MyBookings action is unchanged) ...

        // GET: Bookings (Admin/Staff View - All Bookings)
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ToAirport)
                .Include(b => b.Passenger);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        [Authorize] // Ensure any logged-in user can access this (we filter access inside)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                    .ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight)
                    .ThenInclude(f => f.ToAirport)
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // =========================================================================
            // NEW SECURITY CHECK: Only allow Admin/Staff OR the Booking Owner to view
            // =========================================================================
            if (!User.IsInRole("Admin") && !User.IsInRole("Staff"))
            {
                // 1. Get the current user's ASP.NET Identity ID
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 2. Find the Passenger record for the current user
                var currentUserPassenger = await _context.Passengers
                    .FirstOrDefaultAsync(p => p.UserId == currentUserId);

                // 3. Check if the current user is the owner of the booking
                if (currentUserPassenger == null || booking.PassengerId != currentUserPassenger.Id)
                {
                    // If not an admin/staff AND not the owner, deny access
                    return Forbid(); // Returns a 403 Forbidden status
                }
            }
            // =========================================================================

            return View(booking);
        }

        // ... (Remaining CRUD actions unchanged and still secured by [Authorize(Roles = "Admin,Staff")]) ...

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber");
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "LastName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "LastName", booking.PassengerId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Email", booking.PassengerId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightId,PassengerId,BookingDate,SeatNumber,PricePaid")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Email", booking.PassengerId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        // ... (Existing MyBookings action is unchanged) ...

        [HttpGet]
        [AllowAnonymous]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var passenger = await _context.Passengers
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (passenger == null)
            {
                ViewBag.NoPassengerProfile = true;
                return View("MyBookings", new List<Booking>());
            }

            var userBookings = await _context.Bookings
                .Include(b => b.Flight)
                .ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight)
                .ThenInclude(f => f.ToAirport)
                .Where(b => b.PassengerId == passenger.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(userBookings);
        }
    }
}