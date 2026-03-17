using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        [Required]
        public string? VenueName { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        public int? Capacity { get; set; }
        [Required]
        public string? ImageUrl { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        [Required]
        public ICollection<Booking>? Bookings { get; set; }
    }
}
