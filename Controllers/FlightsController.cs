using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SUPFLY.Data;
using SUPFLY.Models;
using SUPFLY.ViewModels;
using Microsoft.AspNetCore.Identity; // Required for UserManager
using SUPFLY.Areas.Identity.Data; // Required for SUPFLYUser
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SUPFLY.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class FlightsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<SUPFLYUser> _userManager;

        public FlightsController(ApplicationDbContext context, UserManager<SUPFLYUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ----------------------------------------------------------------------------------
        // >>> FLIGHT SEARCH (Public Access) <<<
        // ----------------------------------------------------------------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Search()
        {
            // "Id" is what goes to the database (the value)
            // "Code" is what the user sees in the list (the text)
            ViewData["Airports"] = new SelectList(_context.Airports.OrderBy(a => a.Code), "Id", "Code");

            return View(new FlightSearchViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(FlightSearchViewModel vm)
        {
            // FIX: Use the same pattern as the GET method ("Id", "Code" or "Id", "City")
            // If you want them to see "KHI", use "Code". If you want "Karachi", use "City".
            ViewData["Airports"] = new SelectList(_context.Airports.OrderBy(a => a.City), "Id", "City");

            // 1. Find Outbound Flights
            vm.OutboundFlights = await _context.Flights
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .Where(f => f.FromAirportId == vm.FromAirportId
                         && f.ToAirportId == vm.ToAirportId
                         && f.DepartureTime.Date == vm.DepartureDate.Date)
                .ToListAsync();

            // 2. Find Return Flights (Smart Logic)
            if (vm.IsRoundTrip && vm.ReturnDate.HasValue)
            {
                vm.ReturnFlights = await _context.Flights
                    .Include(f => f.FromAirport)
                    .Include(f => f.ToAirport)
                    .Where(f => f.FromAirportId == vm.ToAirportId  // Reversed for Return
                             && f.ToAirportId == vm.FromAirportId  // Reversed for Return
                             && f.DepartureTime.Date == vm.ReturnDate.Value.Date)
                    .ToListAsync();

                // Prevent "Time Travel"
                if (vm.OutboundFlights.Any())
                {
                    var firstOutboundArrival = vm.OutboundFlights.Min(f => f.ArrivalTime);
                    vm.ReturnFlights = vm.ReturnFlights
                        .Where(rf => rf.DepartureTime > firstOutboundArrival)
                        .ToList();
                }
            }

            // If the search yields no results, we still return the View 
            // The ViewData["Airports"] above ensures the dropdowns stay populated.
            return View(vm);
        }

        // ----------------------------------------------------------------------------------
        // >>> ADMIN ACTIONS <<<
        // ----------------------------------------------------------------------------------

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Flights.Include(f => f.Aircraft).Include(f => f.FromAirport).Include(f => f.ToAirport);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (flight == null) return NotFound();

            return View(flight);
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model");
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code");
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FlightNumber,FromAirportId,ToAirportId,DepartureTime,ArrivalTime,Price")] Flight flight, string AircraftModelCode, string FromAirportCode, string ToAirportCode)
        {
            var fromAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Code == FromAirportCode);
            var toAirport = await _context.Airports.FirstOrDefaultAsync(a => a.Code == ToAirportCode);
            var aircraft = await _context.Aircrafts.FirstOrDefaultAsync(a => a.Model == AircraftModelCode);

            if (fromAirport == null) ModelState.AddModelError("FromAirportCode", "Origin airport code not found.");
            if (toAirport == null) ModelState.AddModelError("ToAirportCode", "Destination airport code not found.");
            if (aircraft == null) ModelState.AddModelError("AircraftModelCode", "Aircraft model not found.");

            if (!ModelState.IsValid || fromAirport == null || toAirport == null || aircraft == null)
            {
                ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model");
                ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code");
                ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code");
                return View(flight);
            }

            flight.FromAirportId = fromAirport.Id;
            flight.ToAirportId = toAirport.Id;
            flight.AircraftId = aircraft.Id;

            _context.Add(flight);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null) return NotFound();

            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.FromAirportId);
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.ToAirportId);
            return View(flight);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightNumber,AircraftId,FromAirportId,ToAirportId,DepartureTime,ArrivalTime,Price")] Flight flight)
        {
            if (id != flight.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.FromAirportId);
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.ToAirportId);
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (flight == null) return NotFound();

            return View(flight);
        }

        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }
    }
}