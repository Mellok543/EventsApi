using EventsApi.DTOs;

namespace EventsApi.Services;

public interface IEventService
{
    IReadOnlyCollection<EventResponse> GetAll();

    EventResponse? GetById(Guid id);

    EventResponse Create(CreateEventRequest request);

    EventResponse? Update(Guid id, UpdateEventRequest request);

    bool Delete(Guid id);
}