using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        [Required(ErrorMessage = "Venue name is required.")]
        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }
        [Required(ErrorMessage = "Location name is required.")]
        [Display(Name = "Location")]
        public string? Location { get; set; }
       
        [Required(ErrorMessage = "Please indicate how much capacity this venue can accomodate.")]
        [Display(Name = "Capacity")]
        public int? Capacity { get; set; }
        
        public string? ImageUrl { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
       
        public ICollection<Booking>? Bookings { get; set; }
    }
}
