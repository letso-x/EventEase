using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService? _azureStorageService;
        private readonly IConfiguration _configuration;

        public EventsController(ApplicationDbContext context, IAzureStorageService? azureStorageService, IConfiguration configuration)
        {
            _context = context;
            _azureStorageService = azureStorageService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            Event? ev = await _context.Events
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            return View(ev);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (_azureStorageService != null)
                        {
                            string containerName = _configuration.GetValue<string>("AzureStorage:ContainerName") ?? "eventease-images";
                            string blobName = await _azureStorageService.UploadFileAsync(imageFile, containerName);
                            if (!string.IsNullOrEmpty(blobName))
                            {
                                var blobUri = await _azureStorageService.GetBlobUriAsync(blobName, containerName);
                                ev.ImageUrl = blobUri?.ToString();
                            }
                        }
                    }

                    _context.Add(ev);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while creating the event: {ex.Message}");
                }
            }
            return View(ev);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var ev = await _context.Events.FindAsync(id);
            return View(ev);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event ev, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (_azureStorageService != null)
                        {
                            string containerName = _configuration.GetValue<string>("AzureStorage:ContainerName") ?? "eventease-images";
                            string blobName = await _azureStorageService.UploadFileAsync(imageFile, containerName);
                            if (!string.IsNullOrEmpty(blobName))
                            {
                                var blobUri = await _azureStorageService.GetBlobUriAsync(blobName, containerName);
                                ev.ImageUrl = blobUri?.ToString();
                            }
                        }
                    }

                    _context.Update(ev);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while updating the event: {ex.Message}");
                }
            }
            return View(ev);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var ev = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev?.Bookings?.Count > 0)
            {
                TempData["WarningMessage"] = $"This event cannot be deleted because it has {ev.Bookings.Count} active booking(s).";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(ev);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var ev = await _context.Events
                    .Include(e => e.Bookings)
                    .FirstOrDefaultAsync(m => m.EventId == id);

                if (ev == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (ev.Bookings?.Count > 0)
                {
                    TempData["ErrorMessage"] = $"This event cannot be deleted because it has {ev.Bookings.Count} active booking(s). Please delete the bookings first.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the event. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}