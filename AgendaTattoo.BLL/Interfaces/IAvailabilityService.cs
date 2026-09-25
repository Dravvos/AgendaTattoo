using AgendaTattoo.DTO.Scheduling;

namespace AgendaTattoo.BLL.Interfaces;

public interface IAvailabilityService
{
    /// <summary>Retorna a grade semanal de horários de um artista. Só o próprio artista ou o dono do estúdio podem consultar.</summary>
    Task<IReadOnlyList<WorkingHoursDto>> GetWorkingHoursAsync(Guid artistId, CancellationToken ct = default);

    /// <summary>Substitui integralmente a grade semanal de horários de um artista.</summary>
    Task SetWorkingHoursAsync(Guid artistId, SetWorkingHoursRequest request, CancellationToken ct = default);

    /// <summary>Calcula os horários livres de um artista para um serviço específico, em uma data específica.</summary>
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid studioId, Guid artistId, Guid serviceId, DateOnly date, CancellationToken ct = default);
}
