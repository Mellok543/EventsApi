using EventsApi.Exceptions;
using EventsApi.DTOs;
using EventsApi.Models;

namespace EventsApi.Services;

public class BookingProcessingBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ExternalSystemDelay = TimeSpan.FromSeconds(2);

    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingProcessingBackgroundService> _logger;

    public BookingProcessingBackgroundService(
        IBookingService bookingService,
        ILogger<BookingProcessingBackgroundService> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessPendingBookingsAsync(stoppingToken);
        }
    }

    private async Task ProcessPendingBookingsAsync(CancellationToken stoppingToken)
    {
        IReadOnlyCollection<BookingResponse> pendingBookings;

        try
        {
            pendingBookings = await _bookingService.GetPendingBookingsAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to get pending bookings");
            return;
        }

        foreach (var booking in pendingBookings)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await Task.Delay(ExternalSystemDelay, stoppingToken);

                var currentBooking = await _bookingService.GetBookingByIdAsync(booking.Id);

                if (currentBooking.Status != BookingStatus.Pending)
                {
                    continue;
                }

                await _bookingService.ConfirmBookingAsync(booking.Id);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (NotFoundException exception)
            {
                _logger.LogWarning(exception, "Pending booking {BookingId} was not found during processing", booking.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process booking {BookingId}", booking.Id);
            }
        }
    }
}
