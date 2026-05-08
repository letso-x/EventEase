using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;
        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? searchQuery, int? bookingId, string? eventName)
        {
            try 
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Venue)
                    .Include(b => b.Event)
                    .ToListAsync();

                // Apply search filters
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    // Try to parse as BookingId if it's numeric
                    if (int.TryParse(searchQuery, out int bookingIdSearch))
                    {
                        bookings = bookings.Where(b => b.BookingId == bookingIdSearch).ToList();
                    }
                    else
                    {
                        // Search by Event Name
                        bookings = bookings.Where(b => 
                            b.Event != null && 
                            b.Event.EventName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                        ).ToList();
                    }
                }
                else if (bookingId.HasValue)
                {
                    bookings = bookings.Where(b => b.BookingId == bookingId.Value).ToList();
                }
                else if (!string.IsNullOrEmpty(eventName))
                {
                    bookings = bookings.Where(b => 
                        b.Event != null && 
                        b.Event.EventName.Contains(eventName, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                var viewModel = new BookingSearchViewModel
                {
                    Bookings = bookings,
                    SearchQuery = searchQuery,
                    SearchBookingId = bookingId,
                    SearchEventName = eventName
                };

                return View(viewModel);
            }
            catch(SqlException ex)
            {
                _logger.LogError("Database timeout: " + ex.Message);
                return View("Error", new ErrorViewModel { Message = "Database is busy." });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
                return View(booking);
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database timeout: " + ex.Message);
                return View("Error", new ErrorViewModel { Message = "Database is busy." });
            }

        }

        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName");
            ViewBag.VenueImages = _context.Venues.ToDictionary(v => v.VenueId, v => v.ImageUrl ?? "");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating booking: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the booking. Please try again.");
                }
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueImages = _context.Venues.ToDictionary(v => v.VenueId, v => v.ImageUrl ?? "");
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueImages = _context.Venues.ToDictionary(v => v.VenueId, v => v.ImageUrl ?? "");
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating booking: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the booking. Please try again.");
                }
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueImages = _context.Venues.ToDictionary(v => v.VenueId, v => v.ImageUrl ?? "");
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(id);
                if (booking != null)
                {
                    _context.Bookings.Remove(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting booking: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the booking. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}