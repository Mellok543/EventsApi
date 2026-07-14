using EventsApi.DTOs;
using EventsApi.Exceptions;
using EventsApi.Models;
using EventsApi.Services;
using Xunit;

namespace EventsApi.Tests;

public class BookingServiceTests
{
    private readonly EventService _eventService = new();
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _bookingService = new BookingService(_eventService);
    }

    private static CreateEventRequest BuildCreateEventRequest(
        string title = "Test event",
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        return new CreateEventRequest
        {
            Title = title,
            Description = "Test description",
            StartAt = startAt ?? new DateTime(2026, 7, 10, 10, 0, 0),
            EndAt = endAt ?? new DateTime(2026, 7, 10, 12, 0, 0)
        };
    }

    [Fact]
    public async Task CreateBookingAsync_WithExistingEvent_ReturnsPendingBooking()
    {
        // Arrange
        var eventItem = _eventService.Create(BuildCreateEventRequest());

        // Act
        var booking = await _bookingService.CreateBookingAsync(eventItem.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(eventItem.Id, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(default, booking.CreatedAt);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_MultipleBookingsForSameEvent_CreatesUniqueIds()
    {
        // Arrange
        var eventItem = _eventService.Create(BuildCreateEventRequest());

        // Act
        var firstBooking = await _bookingService.CreateBookingAsync(eventItem.Id);
        var secondBooking = await _bookingService.CreateBookingAsync(eventItem.Id);
        var thirdBooking = await _bookingService.CreateBookingAsync(eventItem.Id);

        // Assert
        Assert.Equal(3, new[] { firstBooking.Id, secondBooking.Id, thirdBooking.Id }.Distinct().Count());
        Assert.All(
            new[] { firstBooking, secondBooking, thirdBooking },
            booking => Assert.Equal(eventItem.Id, booking.EventId));
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithExistingId_ReturnsBooking()
    {
        // Arrange
        var eventItem = _eventService.Create(BuildCreateEventRequest());
        var createdBooking = await _bookingService.CreateBookingAsync(eventItem.Id);

        // Act
        var booking = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

        // Assert
        Assert.Equal(createdBooking.Id, booking.Id);
        Assert.Equal(eventItem.Id, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_AfterConfirm_ReturnsConfirmedStatus()
    {
        // Arrange
        var eventItem = _eventService.Create(BuildCreateEventRequest());
        var createdBooking = await _bookingService.CreateBookingAsync(eventItem.Id);

        // Act
        await _bookingService.ConfirmBookingAsync(createdBooking.Id);
        var booking = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingByIdAsync_AfterReject_ReturnsRejectedStatus()
    {
        // Arrange
        var eventItem = _eventService.Create(BuildCreateEventRequest());
        var createdBooking = await _bookingService.CreateBookingAsync(eventItem.Id);

        // Act
        await _bookingService.RejectBookingAsync(createdBooking.Id);
        var booking = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_WithNonExistingEvent_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingEventId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.CreateBookingAsync(nonExistingEventId));
    }

    [Fact]
    public async Task CreateBookingAsync_WithDeletedEvent_ThrowsNotFoundException()
    {
        // Arrange
        var eventItem = _eventService.Create(BuildCreateEventRequest());
        _eventService.Delete(eventItem.Id);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.CreateBookingAsync(eventItem.Id));
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingBookingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bookingService.GetBookingByIdAsync(nonExistingBookingId));
    }
}
