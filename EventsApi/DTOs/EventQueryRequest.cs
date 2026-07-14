namespace EventsApi.DTOs;

public class EventQueryRequest
{
    /// <summary>
    /// Поиск по названию (регистронезависимый, частичное совпадение).
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// События, которые начинаются не раньше указанной даты.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// События, которые заканчиваются не позже указанной даты.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Номер страницы (начиная с 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Количество элементов на странице.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
