namespace AgendaTattoo.DTO.Scheduling;

/// <summary>
/// Espelha AgendaTattoo.Data.Entities.AppointmentStatus com os mesmos valores numéricos.
/// Mantido separado para o projeto DTO não depender do projeto Data (mapeado por cast explícito na BLL).
/// </summary>
public enum AppointmentStatusDto
{
    PendingConfirmation = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    NoShow = 4,
}
