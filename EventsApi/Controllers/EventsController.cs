using EventsApi.DTOs;
using EventsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<EventResponse>> GetAll()
    {
        var events = _eventService.GetAll();

        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<EventResponse> GetById(Guid id)
    {
        var eventItem = _eventService.GetById(id);

        if (eventItem is null)
        {
            return NotFound();
        }

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

        if (updatedEvent is null)
        {
            return NotFound();
        }

        return Ok(updatedEvent);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var deleted = _eventService.Delete(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}