using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;

namespace AgendaTattoo.BLL.Interfaces;

/// <summary>Fluxo público de agendamento (sem login), acessado pelo slug do estúdio.</summary>
public interface IPublicBookingService
{
    Task<PublicStudioDto> GetStudioAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<PublicServiceDto>> ListServicesAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<PublicArtistDto>> ListArtistsAsync(string slug, CancellationToken ct = default);

    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        string slug, Guid artistId, Guid serviceId, DateOnly date, CancellationToken ct = default);

    Task<PublicBookingResponse> CreateBookingAsync(string slug, CreatePublicBookingRequest request, CancellationToken ct = default);
}
