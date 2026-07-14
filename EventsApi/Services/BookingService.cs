using System.Collections.Concurrent;
using EventsApi.DTOs;
using EventsApi.Exceptions;
using EventsApi.Models;

namespace EventsApi.Services;

public class BookingService : IBookingService
{
    private readonly IEventService _eventService;
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public BookingService(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Task<BookingResponse> CreateBookingAsync(Guid eventId)
    {
        _eventService.GetById(eventId);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _bookings[booking.Id] = booking;

        return Task.FromResult(MapToResponse(booking));
    }

    public Task<BookingResponse> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = FindById(bookingId);

        return Task.FromResult(MapToResponse(booking));
    }

    public Task<IReadOnlyCollection<BookingResponse>> GetPendingBookingsAsync()
    {
        var pendingBookings = _bookings.Values
            .Where(booking => booking.Status == BookingStatus.Pending)
            .Select(MapToResponse)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<BookingResponse>>(pendingBookings);
    }

    public Task<BookingResponse> ConfirmBookingAsync(Guid bookingId)
    {
        var booking = UpdateStatus(bookingId, BookingStatus.Confirmed);

        return Task.FromResult(MapToResponse(booking));
    }

    public Task<BookingResponse> RejectBookingAsync(Guid bookingId)
    {
        var booking = UpdateStatus(bookingId, BookingStatus.Rejected);

        return Task.FromResult(MapToResponse(booking));
    }

    private Booking UpdateStatus(Guid bookingId, BookingStatus status)
    {
        var booking = FindById(bookingId);

        booking.Status = status;
        booking.ProcessedAt = DateTime.UtcNow;

        return booking;
    }

    private Booking FindById(Guid bookingId)
    {
        if (!_bookings.TryGetValue(bookingId, out var booking))
        {
            throw new NotFoundException($"Booking with id '{bookingId}' was not found");
        }

        return booking;
    }

    private static BookingResponse MapToResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
    }
}
