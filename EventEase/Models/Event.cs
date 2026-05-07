using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }
        [Required(ErrorMessage = "Event name is required.")]
        [Display(Name ="Event Name")]
        public string EventName { get; set; }
        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Please choose a start date.")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        [Required(ErrorMessage = "Please choose an end date.")]
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}