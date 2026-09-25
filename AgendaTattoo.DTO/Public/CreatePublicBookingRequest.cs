using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Public;

/// <summary>Agendamento feito pelo próprio cliente, sem login, pelo link público do estúdio.</summary>
public class CreatePublicBookingRequest
{
    [Required]
    public Guid ArtistId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateTimeOffset StartsAt { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    public string ClientFullName { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string ClientPhoneNumber { get; set; } = string.Empty;

    [EmailAddress, StringLength(200)]
    public string? ClientEmail { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
