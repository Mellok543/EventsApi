using EventsApi.DTOs;
using EventsApi.Models;

namespace EventsApi.Services;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(Guid eventId);

    Task<BookingResponse> GetBookingByIdAsync(Guid bookingId);

    Task<IReadOnlyCollection<BookingResponse>> GetPendingBookingsAsync();

    Task<BookingResponse> ConfirmBookingAsync(Guid bookingId);

    Task<BookingResponse> RejectBookingAsync(Guid bookingId);
}
