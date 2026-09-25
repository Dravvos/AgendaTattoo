using AgendaTattoo.DTO.Scheduling;

namespace AgendaTattoo.BLL.Interfaces;

/// <summary>Agenda interna: donos veem/gerenciam tudo do estúdio, artistas veem/gerenciam só a própria agenda.</summary>
public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentDto>> ListAsync(DateTimeOffset from, DateTimeOffset to, Guid? artistId, CancellationToken ct = default);
    Task<AppointmentDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken ct = default);
    Task<AppointmentDto> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken ct = default);
    Task<AppointmentDto> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, CancellationToken ct = default);
}
