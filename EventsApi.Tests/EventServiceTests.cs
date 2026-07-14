using EventsApi.DTOs;
using EventsApi.Exceptions;
using EventsApi.Services;
using Xunit;

namespace EventsApi.Tests;

public class EventServiceTests
{
    private readonly EventService _service = new();

    private static CreateEventRequest BuildCreateRequest(
        string? title = "Test event",
        string? description = "Test description",
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        return new CreateEventRequest
        {
            Title = title,
            Description = description,
            StartAt = startAt ?? new DateTime(2026, 7, 10, 10, 0, 0),
            EndAt = endAt ?? new DateTime(2026, 7, 10, 12, 0, 0)
        };
    }

    private static UpdateEventRequest BuildUpdateRequest(
        string? title = "Updated event",
        string? description = "Updated description",
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        return new UpdateEventRequest
        {
            Title = title,
            Description = description,
            StartAt = startAt ?? new DateTime(2026, 7, 15, 14, 0, 0),
            EndAt = endAt ?? new DateTime(2026, 7, 15, 16, 0, 0)
        };
    }

    private static EventQueryRequest BuildQuery(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        return new EventQueryRequest
        {
            Title = title,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };
    }

    // --- Успешные сценарии ---

    [Fact]
    public void Create_WithValidData_ReturnsCreatedEvent()
    {
        // Arrange
        var request = BuildCreateRequest(title: "  Conference  ");

        // Act
        var result = _service.Create(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Conference", result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
    }

    [Fact]
    public void GetAll_WithoutFilters_ReturnsAllEvents()
    {
        // Arrange
        _service.Create(BuildCreateRequest(title: "First"));
        _service.Create(BuildCreateRequest(title: "Second"));
        _service.Create(BuildCreateRequest(title: "Third"));

        // Act
        var result = _service.GetAll(BuildQuery());

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public void GetById_WithExistingId_ReturnsEvent()
    {
        // Arrange
        var created = _service.Create(BuildCreateRequest(title: "Meetup"));

        // Act
        var result = _service.GetById(created.Id);

        // Assert
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Meetup", result.Title);
    }

    [Fact]
    public void Update_WithExistingId_UpdatesEvent()
    {
        // Arrange
        var created = _service.Create(BuildCreateRequest(title: "Old title"));
        var request = BuildUpdateRequest(title: "New title");

        // Act
        var result = _service.Update(created.Id, request);

        // Assert
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("New title", result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
    }

    [Fact]
    public void Delete_WithExistingId_RemovesEvent()
    {
        // Arrange
        var created = _service.Create(BuildCreateRequest());

        // Act
        _service.Delete(created.Id);

        // Assert
        Assert.Throws<NotFoundException>(() => _service.GetById(created.Id));
        Assert.Equal(0, _service.GetAll(BuildQuery()).TotalCount);
    }

    [Fact]
    public void GetAll_FilterByTitle_ReturnsCaseInsensitivePartialMatches()
    {
        // Arrange
        _service.Create(BuildCreateRequest(title: "Tech Conference"));
        _service.Create(BuildCreateRequest(title: "Music Festival"));
        _service.Create(BuildCreateRequest(title: "CONFERENCE Call"));

        // Act
        var result = _service.GetAll(BuildQuery(title: "conference"));

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, e =>
            Assert.Contains("conference", e.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetAll_FilterByFrom_ReturnsEventsStartingAtOrAfterDate()
    {
        // Arrange
        _service.Create(BuildCreateRequest(
            title: "Early",
            startAt: new DateTime(2026, 7, 1, 10, 0, 0),
            endAt: new DateTime(2026, 7, 1, 12, 0, 0)));
        _service.Create(BuildCreateRequest(
            title: "Late",
            startAt: new DateTime(2026, 7, 20, 10, 0, 0),
            endAt: new DateTime(2026, 7, 20, 12, 0, 0)));

        // Act
        var result = _service.GetAll(BuildQuery(from: new DateTime(2026, 7, 10)));

        // Assert
        var eventItem = Assert.Single(result.Items);
        Assert.Equal("Late", eventItem.Title);
    }

    [Fact]
    public void GetAll_FilterByTo_ReturnsEventsEndingAtOrBeforeDate()
    {
        // Arrange
        _service.Create(BuildCreateRequest(
            title: "Early",
            startAt: new DateTime(2026, 7, 1, 10, 0, 0),
            endAt: new DateTime(2026, 7, 1, 12, 0, 0)));
        _service.Create(BuildCreateRequest(
            title: "Late",
            startAt: new DateTime(2026, 7, 20, 10, 0, 0),
            endAt: new DateTime(2026, 7, 20, 12, 0, 0)));

        // Act
        var result = _service.GetAll(BuildQuery(to: new DateTime(2026, 7, 10)));

        // Assert
        var eventItem = Assert.Single(result.Items);
        Assert.Equal("Early", eventItem.Title);
    }

    [Fact]
    public void GetAll_FilterByFrom_IncludesEventStartingExactlyAtBoundary()
    {
        // Arrange
        var boundary = new DateTime(2026, 7, 10, 10, 0, 0);
        _service.Create(BuildCreateRequest(
            title: "Boundary",
            startAt: boundary,
            endAt: boundary.AddHours(2)));

        // Act
        var result = _service.GetAll(BuildQuery(from: boundary));

        // Assert
        var eventItem = Assert.Single(result.Items);
        Assert.Equal("Boundary", eventItem.Title);
    }

    [Fact]
    public void GetAll_Pagination_ReturnsRequestedPage()
    {
        // Arrange
        for (var i = 1; i <= 5; i++)
        {
            _service.Create(BuildCreateRequest(title: $"Event {i}"));
        }

        // Act
        var result = _service.GetAll(BuildQuery(page: 2, pageSize: 2));

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);
        Assert.Collection(
            result.Items,
            e => Assert.Equal("Event 3", e.Title),
            e => Assert.Equal("Event 4", e.Title));
    }

    [Fact]
    public void GetAll_PaginationLastPage_ReturnsRemainingEvents()
    {
        // Arrange
        for (var i = 1; i <= 5; i++)
        {
            _service.Create(BuildCreateRequest(title: $"Event {i}"));
        }

        // Act
        var result = _service.GetAll(BuildQuery(page: 3, pageSize: 2));

        // Assert
        Assert.Equal(5, result.TotalCount);
        var eventItem = Assert.Single(result.Items);
        Assert.Equal("Event 5", eventItem.Title);
    }

    [Fact]
    public void GetAll_PaginationPageBeyondData_ReturnsEmptyItems()
    {
        // Arrange
        _service.Create(BuildCreateRequest());

        // Act
        var result = _service.GetAll(BuildQuery(page: 10, pageSize: 10));

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void GetAll_CombinedFilters_AppliesAllFiltersTogether()
    {
        // Arrange
        _service.Create(BuildCreateRequest(
            title: "Tech Conference",
            startAt: new DateTime(2026, 7, 5, 10, 0, 0),
            endAt: new DateTime(2026, 7, 5, 12, 0, 0)));
        _service.Create(BuildCreateRequest(
            title: "Tech Conference",
            startAt: new DateTime(2026, 8, 5, 10, 0, 0),
            endAt: new DateTime(2026, 8, 5, 12, 0, 0)));
        _service.Create(BuildCreateRequest(
            title: "Music Festival",
            startAt: new DateTime(2026, 7, 5, 10, 0, 0),
            endAt: new DateTime(2026, 7, 5, 12, 0, 0)));

        // Act
        var result = _service.GetAll(BuildQuery(
            title: "tech",
            from: new DateTime(2026, 7, 1),
            to: new DateTime(2026, 7, 31)));

        // Assert
        var eventItem = Assert.Single(result.Items);
        Assert.Equal("Tech Conference", eventItem.Title);
        Assert.Equal(new DateTime(2026, 7, 5, 10, 0, 0), eventItem.StartAt);
    }

    [Fact]
    public void GetAll_TitleFilterIsWhitespace_ReturnsAllEvents()
    {
        // Arrange
        _service.Create(BuildCreateRequest(title: "First"));
        _service.Create(BuildCreateRequest(title: "Second"));

        // Act
        var result = _service.GetAll(BuildQuery(title: "   "));

        // Assert
        Assert.Equal(2, result.TotalCount);
    }

    // --- Неуспешные сценарии ---

    [Fact]
    public void GetById_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<NotFoundException>(
            () => _service.GetById(nonExistingId));
        Assert.Contains(nonExistingId.ToString(), exception.Message);
    }

    [Fact]
    public void Update_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var request = BuildUpdateRequest();

        // Act & Assert
        Assert.Throws<NotFoundException>(
            () => _service.Update(nonExistingId, request));
    }

    [Fact]
    public void Delete_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<NotFoundException>(
            () => _service.Delete(nonExistingId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsValidationException(string? title)
    {
        // Arrange
        var request = BuildCreateRequest(title: title);

        // Act & Assert
        Assert.Throws<ValidationException>(() => _service.Create(request));
    }

    [Fact]
    public void Create_WithEndAtBeforeStartAt_ThrowsValidationException()
    {
        // Arrange
        var request = BuildCreateRequest(
            startAt: new DateTime(2026, 7, 10, 12, 0, 0),
            endAt: new DateTime(2026, 7, 10, 10, 0, 0));

        // Act & Assert
        Assert.Throws<ValidationException>(() => _service.Create(request));
    }

    [Fact]
    public void Create_WithEndAtEqualToStartAt_ThrowsValidationException()
    {
        // Arrange
        var date = new DateTime(2026, 7, 10, 10, 0, 0);
        var request = BuildCreateRequest(startAt: date, endAt: date);

        // Act & Assert
        Assert.Throws<ValidationException>(() => _service.Create(request));
    }

    [Fact]
    public void Update_WithEndAtBeforeStartAt_ThrowsValidationException()
    {
        // Arrange
        var created = _service.Create(BuildCreateRequest());
        var request = BuildUpdateRequest(
            startAt: new DateTime(2026, 7, 15, 16, 0, 0),
            endAt: new DateTime(2026, 7, 15, 14, 0, 0));

        // Act & Assert
        Assert.Throws<ValidationException>(
            () => _service.Update(created.Id, request));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public void GetAll_WithInvalidPagination_ThrowsValidationException(int page, int pageSize)
    {
        // Arrange
        var query = BuildQuery(page: page, pageSize: pageSize);

        // Act & Assert
        Assert.Throws<ValidationException>(() => _service.GetAll(query));
    }
}
