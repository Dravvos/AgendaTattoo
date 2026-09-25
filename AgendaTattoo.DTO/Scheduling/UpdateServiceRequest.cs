using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Scheduling;

public class UpdateServiceRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(5, 24 * 60)]
    public int DurationMinutes { get; set; }

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
