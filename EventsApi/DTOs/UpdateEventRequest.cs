using System.ComponentModel.DataAnnotations;

namespace EventsApi.DTOs;

public class UpdateEventRequest : IValidatableObject
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(1, ErrorMessage = "Title cannot be empty")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "StartAt is required")]
    public DateTime? StartAt { get; set; }

    [Required(ErrorMessage = "EndAt is required")]
    public DateTime? EndAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartAt.HasValue && EndAt.HasValue && EndAt <= StartAt)
        {
            yield return new ValidationResult(
                "EndAt must be later than StartAt",
                new[] { nameof(EndAt) });
        }
    }
}