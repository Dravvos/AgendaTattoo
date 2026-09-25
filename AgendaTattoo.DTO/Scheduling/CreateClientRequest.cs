using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Scheduling;

public class CreateClientRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
