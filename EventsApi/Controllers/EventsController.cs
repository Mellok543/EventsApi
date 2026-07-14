using EventsApi.DTOs;
using EventsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;

    public EventsController(
        IEventService eventService,
        IBookingService bookingService)
    {
        _eventService = eventService;
        _bookingService = bookingService;
    }

    [HttpGet]
    public ActionResult<PaginatedResult<EventResponse>> GetAll([FromQuery] EventQueryRequest query)
    {
        var events = _eventService.GetAll(query);

        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<EventResponse> GetById(Guid id)
    {
        var eventItem = _eventService.GetById(id);

        return Ok(eventItem);
    }

    [HttpPost]
    public ActionResult<EventResponse> Create(CreateEventRequest request)
    {
        var createdEvent = _eventService.Create(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdEvent.Id },
            createdEvent);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<EventResponse> Update(Guid id, UpdateEventRequest request)
    {
        var updatedEvent = _eventService.Update(id, request);

        return Ok(updatedEvent);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventService.Delete(id);

        return NoContent();
    }

    [HttpPost("{id:guid}/book")]
    public async Task<ActionResult<BookingResponse>> Book(Guid id)
    {
        var booking = await _bookingService.CreateBookingAsync(id);

        return AcceptedAtAction(
            nameof(BookingsController.GetById),
            "Bookings",
            new { id = booking.Id },
            booking);
    }
}
