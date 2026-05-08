using System.ComponentModel.DataAnnotations;
using System.Globalization;
using EventEase.Models.Validation;

namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        [Required(ErrorMessage = "Please select an available venue.")]
        public int VenueId { get; set; }
        [Required(ErrorMessage = "Please select an event to associate with this booking.")]
        public int EventId { get; set; }
        [Required(ErrorMessage = "Please select a booking date")]
        [Display(Name ="Booking Date")]
        [UniqueVenueBooking(ErrorMessage = "This venue is already booked for the selected date.")]
        public DateTime BookingDate { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        [Required(ErrorMessage ="Please select an available venue.")]
        public Venue? Venue { get; set; }
        
        public Event? Event { get; set; }
    }
}