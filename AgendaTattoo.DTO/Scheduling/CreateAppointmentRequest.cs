using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Scheduling;

/// <summary>Criação de agendamento pela equipe do estúdio (ex.: cliente ligou/apareceu pessoalmente).</summary>
public class CreateAppointmentRequest
{
    [Required]
    public Guid ArtistId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateTimeOffset StartsAt { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
