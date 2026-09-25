using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Scheduling;

public class RescheduleAppointmentRequest
{
    [Required]
    public DateTimeOffset StartsAt { get; set; }

    /// <summary>Opcional: transferir o agendamento para outro artista ao reagendar.</summary>
    public Guid? ArtistId { get; set; }
}
