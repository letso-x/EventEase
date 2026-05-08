namespace EventEase.Models
{
    public class BookingSearchViewModel
    {
        public List<Booking> Bookings { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? SearchBookingId { get; set; }
        public string? SearchEventName { get; set; }
    }
}
