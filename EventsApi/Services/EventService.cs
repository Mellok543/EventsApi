using EventsApi.DTOs;
using EventsApi.Models;

namespace EventsApi.Services;

public class EventService : IEventService
{
    private readonly List<Event> _events = new();

    public IReadOnlyCollection<EventResponse> GetAll()
    {
        return _events
            .Select(MapToResponse)
            .ToList();
    }

    public EventResponse? GetById(Guid id)
    {
        var eventItem = _events.FirstOrDefault(e => e.Id == id);

        return eventItem is null
            ? null
            : MapToResponse(eventItem);
    }

    public EventResponse Create(CreateEventRequest request)
    {
        var eventItem = new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            Description = request.Description,
            StartAt = request.StartAt!.Value,
            EndAt = request.EndAt!.Value
        };

        _events.Add(eventItem);

        return MapToResponse(eventItem);
    }

    public EventResponse? Update(Guid id, UpdateEventRequest request)
    {
        var eventItem = _events.FirstOrDefault(e => e.Id == id);

        if (eventItem is null)
        {
            return null;
        }

        eventItem.Title = request.Title!.Trim();
        eventItem.Description = request.Description;
        eventItem.StartAt = request.StartAt!.Value;
        eventItem.EndAt = request.EndAt!.Value;

        return MapToResponse(eventItem);
    }

    public bool Delete(Guid id)
    {
        var eventItem = _events.FirstOrDefault(e => e.Id == id);

        if (eventItem is null)
        {
            return false;
        }

        _events.Remove(eventItem);
        return true;
    }

    private static EventResponse MapToResponse(Event eventItem)
    {
        return new EventResponse
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            StartAt = eventItem.StartAt,
            EndAt = eventItem.EndAt
        };
    }
}