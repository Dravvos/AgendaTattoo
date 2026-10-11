using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;

namespace AgendaTattoo.BLL.Interfaces;

public interface IAvailabilityService
{
    /// <summary>Retorna a grade semanal de horários de um artista. Só o próprio artista ou o dono do estúdio podem consultar.</summary>
    Task<IReadOnlyList<WorkingHoursDto>> GetWorkingHoursAsync(Guid artistId, CancellationToken ct = default);

    /// <summary>
    /// Substitui integralmente a grade semanal de horários de um artista. Cada dia pode ter mais
    /// de uma faixa (ex.: 09:00–12:00 e 13:00–18:00, com intervalo de almoço entre elas), desde
    /// que as faixas do mesmo dia não se sobreponham.
    /// </summary>
    Task SetWorkingHoursAsync(Guid artistId, SetWorkingHoursRequest request, CancellationToken ct = default);

    /// <summary>Calcula os horários livres de um artista para um serviço específico, em uma data específica.</summary>
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid studioId, Guid artistId, Guid serviceId, DateOnly date, CancellationToken ct = default);

    /// <summary>Lista as folgas pontuais do artista dentro do período [from, to] (inclusive).</summary>
    Task<IReadOnlyList<ArtistTimeOffDto>> ListTimeOffAsync(
        Guid artistId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>Registra uma folga pontual (dia inteiro ou intervalo específico numa data).</summary>
    Task<ArtistTimeOffDto> AddTimeOffAsync(Guid artistId, CreateArtistTimeOffRequest request, CancellationToken ct = default);

    /// <summary>Remove uma folga pontual já registrada.</summary>
    Task RemoveTimeOffAsync(Guid artistId, Guid timeOffId, CancellationToken ct = default);
    Task<PublicAvailabilityCalendarDto> GetAvailabilityCalendarAsync(
    Guid studioId, Guid artistId, DateOnly from, DateOnly to, CancellationToken ct = default);
}
