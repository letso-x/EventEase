using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int VenueId { get; set; }
        public int EventId { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }

        public Venue? Venue { get; set; }
        public Event? Event { get; set; }
    }
}