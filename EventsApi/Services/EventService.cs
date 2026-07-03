using EventsApi.DTOs;
using EventsApi.Exceptions;
using EventsApi.Models;

namespace EventsApi.Services;

public class EventService : IEventService
{
    private readonly List<Event> _events = new();

    public PaginatedResult<EventResponse> GetAll(EventQueryRequest query)
    {
        ValidatePagination(query);

        var filteredEvents = _events.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            filteredEvents = filteredEvents.Where(e =>
                e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (query.From.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.StartAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            filteredEvents = filteredEvents.Where(e => e.EndAt <= query.To.Value);
        }

        var filteredList = filteredEvents.ToList();

        var items = filteredList
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToResponse)
            .ToList();

        return new PaginatedResult<EventResponse>
        {
            Items = items,
            TotalCount = filteredList.Count,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public EventResponse GetById(Guid id)
    {
        var eventItem = FindById(id);

        return MapToResponse(eventItem);
    }

    public EventResponse Create(CreateEventRequest request)
    {
        ValidateEventData(request.Title, request.StartAt, request.EndAt);

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

    public EventResponse Update(Guid id, UpdateEventRequest request)
    {
        var eventItem = FindById(id);

        ValidateEventData(request.Title, request.StartAt, request.EndAt);

        eventItem.Title = request.Title!.Trim();
        eventItem.Description = request.Description;
        eventItem.StartAt = request.StartAt!.Value;
        eventItem.EndAt = request.EndAt!.Value;

        return MapToResponse(eventItem);
    }

    public void Delete(Guid id)
    {
        var eventItem = FindById(id);

        _events.Remove(eventItem);
    }

    private Event FindById(Guid id)
    {
        var eventItem = _events.FirstOrDefault(e => e.Id == id);

        if (eventItem is null)
        {
            throw new NotFoundException($"Event with id '{id}' was not found");
        }

        return eventItem;
    }

    private static void ValidatePagination(EventQueryRequest query)
    {
        if (query.Page < 1)
        {
            throw new ValidationException("Page must be greater than or equal to 1");
        }

        if (query.PageSize < 1)
        {
            throw new ValidationException("PageSize must be greater than or equal to 1");
        }
    }

    private static void ValidateEventData(string? title, DateTime? startAt, DateTime? endAt)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Title is required");
        }

        if (!startAt.HasValue)
        {
            throw new ValidationException("StartAt is required");
        }

        if (!endAt.HasValue)
        {
            throw new ValidationException("EndAt is required");
        }

        if (endAt <= startAt)
        {
            throw new ValidationException("EndAt must be later than StartAt");
        }
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
