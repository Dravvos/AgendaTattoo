namespace AgendaTattoo.DTO.Public;

public class PublicBookingResponse
{
    public Guid AppointmentId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
