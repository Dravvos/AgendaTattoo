using AgendaTattoo.DTO.Scheduling;

namespace AgendaTattoo.BLL.Interfaces;

/// <summary>Catálogo de serviços (tipos de sessão) oferecidos pelo estúdio do usuário autenticado.</summary>
public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken ct = default);
    Task<ServiceDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<ServiceDto> CreateAsync(CreateServiceRequest request, CancellationToken ct = default);
    Task<ServiceDto> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
}
