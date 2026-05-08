using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService? _azureStorageService;
        private readonly IConfiguration _configuration;

        public VenuesController(ApplicationDbContext context, IAzureStorageService? azureStorageService, IConfiguration configuration)
        {
            _context = context;
            _azureStorageService = azureStorageService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
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
                                venue.ImageUrl = blobUri?.ToString();
                            }
                        }
                    }

                    _context.Add(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while creating the venue: {ex.Message}");
                }
            }
            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var venue = await _context.Venues.FindAsync(id);
            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile)
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
                                venue.ImageUrl = blobUri?.ToString();
                            }
                        }
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while updating the venue: {ex.Message}");
                }
            }
            return View(venue);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue?.Bookings?.Count > 0)
            {
                TempData["WarningMessage"] = $"This venue cannot be deleted because it has {venue.Bookings.Count} active booking(s).";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var venue = await _context.Venues
                    .Include(v => v.Bookings)
                    .FirstOrDefaultAsync(m => m.VenueId == id);

                if (venue == null)
                {
                    TempData["ErrorMessage"] = "Venue not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (venue.Bookings?.Count > 0)
                {
                    TempData["ErrorMessage"] = $"This venue cannot be deleted because it has {venue.Bookings.Count} active booking(s). Please delete the bookings first.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue deleted successfully!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the venue. Please try again.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}