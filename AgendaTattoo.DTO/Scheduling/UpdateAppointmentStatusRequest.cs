using System.ComponentModel.DataAnnotations;

namespace AgendaTattoo.DTO.Scheduling;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public AppointmentStatusDto Status { get; set; }
}
