using Microsoft.AspNetCore.Mvc;
using SUPFLY.Data;
using SUPFLY.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace SUPFLY.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Search
        public IActionResult Index()
        {
            return View(new BookingSearchModel());
        }

        // POST: /Search/FindFlights
        [HttpPost]
        public IActionResult FindFlights(BookingSearchModel model)
        {
            if (ModelState.IsValid)
            {
                var originAirport = _context.Airports.FirstOrDefault(a => a.Code == model.Origin);
                var destinationAirport = _context.Airports.FirstOrDefault(a => a.Code == model.Destination);

                if (originAirport == null || destinationAirport == null)
                {
                    ModelState.AddModelError(string.Empty, "The specified origin or destination airport code was not found.");
                    return View("Index", model);
                }

                var flights = _context.Flights
                    .Include(f => f.Aircraft)
                    .Include(f => f.FromAirport)
                    .Include(f => f.ToAirport)
                    .Where(f => f.FromAirportId == originAirport.Id &&
                                f.ToAirportId == destinationAirport.Id &&
                                f.DepartureTime.Date == model.DepartureDate.Date)
                    .ToList();

                ViewData["SearchModel"] = model;
                return View("Results", flights);
            }
            return View("Index", model);
        }

        // GET: /Search/Book?flightId=5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Book(int flightId)
        {
            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(m => m.Id == flightId);

            if (flight == null)
            {
                return NotFound();
            }

            // =========================================================================
            // NEW LOGIC: Fetch all already booked seats for this flight
            // =========================================================================
            var bookedSeats = await _context.Bookings
                .Where(b => b.FlightId == flightId)
                .Select(b => b.SeatNumber)
                .ToListAsync();

            // Store the booked seats in ViewData to pass to the view
            ViewData["BookedSeats"] = bookedSeats;
            // =========================================================================

            return View(flight);
        }

        // POST: /Search/Book (No change needed here as seat validation is already added)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Book(int flightId, [Bind("SeatNumber")] Booking model)
        {
            var flight = await _context.Flights.FindAsync(flightId);

            if (flight == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (passenger == null)
            {
                ModelState.AddModelError(string.Empty, "Passenger profile not found. Please ensure you have created your profile.");
                var flightForError = await _context.Flights
                    .Include(f => f.Aircraft).Include(f => f.FromAirport).Include(f => f.ToAirport)
                    .FirstOrDefaultAsync(m => m.Id == flightId);
                return View("Book", flightForError);
            }

            var seatConflict = await _context.Bookings
                .AnyAsync(b => b.FlightId == flightId && b.SeatNumber == model.SeatNumber);

            if (seatConflict)
            {
                ModelState.AddModelError("SeatNumber", "This seat is already booked for this flight. Please choose a different one.");

                var flightForError = await _context.Flights
                    .Include(f => f.Aircraft).Include(f => f.FromAirport).Include(f => f.ToAirport)
                    .FirstOrDefaultAsync(m => m.Id == flightId);

                // Re-fetch booked seats to show the user on error
                ViewData["BookedSeats"] = await _context.Bookings
                    .Where(b => b.FlightId == flightId)
                    .Select(b => b.SeatNumber)
                    .ToListAsync();

                return View("Book", flightForError);
            }

            model.FlightId = flightId;
            model.PassengerId = passenger.Id;
            model.BookingDate = System.DateTime.Now;
            model.PricePaid = flight.Price;

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingConfirmation", new { id = model.Id });
        }

        // GET: /Search/BookingConfirmation/5 (No change)
        [Authorize]
        public async Task<IActionResult> BookingConfirmation(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .ThenInclude(f => f.FromAirport)
                .Include(b => b.Flight)
                .ThenInclude(f => f.ToAirport)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
    }
}