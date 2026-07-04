using EventsApi.DTOs;

namespace EventsApi.Services;

public interface IEventService
{
    PaginatedResult<EventResponse> GetAll(EventQueryRequest query);

    EventResponse GetById(Guid id);

    EventResponse Create(CreateEventRequest request);

    EventResponse Update(Guid id, UpdateEventRequest request);

    void Delete(Guid id);
}
