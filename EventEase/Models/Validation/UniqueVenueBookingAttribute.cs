using System.ComponentModel.DataAnnotations;

namespace EventEase.Models.Validation
{
    public class UniqueVenueBookingAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var booking = validationContext.ObjectInstance as Booking;
            if (booking == null)
                return ValidationResult.Success;

            // Get the ApplicationDbContext from the validation context
            var context = validationContext.GetService(typeof(EventEase.Data.ApplicationDbContext)) as EventEase.Data.ApplicationDbContext;
            if (context == null)
                return ValidationResult.Success;

            // Check if a booking already exists for the same venue on the same date (excluding current booking if editing)
            var existingBooking = context.Bookings
                .FirstOrDefault(b => 
                    b.VenueId == booking.VenueId && 
                    b.BookingDate.Date == booking.BookingDate.Date &&
                    b.BookingId != booking.BookingId); // Exclude current booking when editing

            if (existingBooking != null)
            {
                return new ValidationResult($"The venue '{existingBooking.Venue?.VenueName}' is already booked for {booking.BookingDate:yyyy-MM-dd}. Please select a different date or venue.");
            }

            return ValidationResult.Success;
        }
    }
}
