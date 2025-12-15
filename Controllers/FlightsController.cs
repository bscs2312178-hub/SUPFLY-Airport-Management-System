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

namespace SUPFLY.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class FlightsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Flights
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Flights.Include(f => f.Aircraft).Include(f => f.FromAirport).Include(f => f.ToAirport);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Flights/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flights/Create
        public IActionResult Create()
        {
            // ✅ FIX: Use consistent DbSet names for dropdowns if needed
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model");
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code");
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    // We bind the Flight object, but ignore the AircraftId property from binding
    [Bind("FlightNumber,FromAirportId,ToAirportId,DepartureTime,ArrivalTime,Price")] Flight flight,
    string AircraftModelCode, // ADDED: New parameter to capture the string code
    string FromAirportCode,
    string ToAirportCode)
        {
            // 1. Find the Airport IDs based on the codes entered by the user
            var fromAirport = await _context.Airports
                .FirstOrDefaultAsync(a => a.Code == FromAirportCode);

            var toAirport = await _context.Airports
                .FirstOrDefaultAsync(a => a.Code == ToAirportCode);

            // 2. Find the Aircraft ID based on the string code entered by the user
            // ✅ FIX: Compare a.Model (string) with the new string parameter (AircraftModelCode)
            var aircraft = await _context.Aircrafts
                .FirstOrDefaultAsync(a => a.Model == AircraftModelCode);

            // Validation Check: Ensure the codes are valid
            if (fromAirport == null)
            {
                ModelState.AddModelError("FromAirportCode", "Origin airport code not found.");
            }
            if (toAirport == null)
            {
                ModelState.AddModelError("ToAirportCode", "Destination airport code not found.");
            }
            if (aircraft == null)
            {
                // Use the new string parameter name for the error key
                ModelState.AddModelError("AircraftModelCode", "Aircraft model not found.");
            }

            // If validation fails, return to the view with errors.
            if (!ModelState.IsValid || fromAirport == null || toAirport == null || aircraft == null)
            {
                ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
                ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.FromAirportId);
                ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.ToAirportId);

                return View(flight);
            }

            // 3. Assign the actual foreign key IDs to the Flight object
            flight.FromAirportId = fromAirport.Id;
            flight.ToAirportId = toAirport.Id;
            // Assign the actual Aircraft ID (integer) from the found 'aircraft' object.
            flight.AircraftId = aircraft.Id;

            // Clear any previous errors that might have been related to the missing IDs
            ModelState.Clear();

            // Final check for model state (should be valid now)
            if (ModelState.IsValid)
            {
                _context.Add(flight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Fallback if something else went wrong
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.FromAirportId);
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.ToAirportId);
            return View(flight);
        }


        // GET: Flights/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            // ✅ FIX: Use Model/Code consistently in Edit
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.FromAirportId);
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.ToAirportId);
            return View(flight);
        }

        // POST: Flights/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightNumber,AircraftId,FromAirportId,ToAirportId,DepartureTime,ArrivalTime,Price")] Flight flight)
        {
            if (id != flight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id))
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
            // ✅ FIX: Use Model/Code consistently in Edit error return
            ViewData["AircraftId"] = new SelectList(_context.Aircrafts, "Id", "Model", flight.AircraftId);
            ViewData["FromAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.FromAirportId);
            ViewData["ToAirportId"] = new SelectList(_context.Airports, "Id", "Code", flight.ToAirportId);
            return View(flight);
        }

        // GET: Flights/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.FromAirport)
                .Include(f => f.ToAirport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (flight == null)
            {
                return NotFound();
            }

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