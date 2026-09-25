using AgendaTattoo.DTO.Scheduling;

namespace AgendaTattoo.BLL.Interfaces;

/// <summary>Cadastro de clientes do estúdio do usuário autenticado.</summary>
public interface IClientService
{
    Task<IReadOnlyList<ClientDto>> ListAsync(string? search, CancellationToken ct = default);
    Task<ClientDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default);
    Task<ClientDto> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default);
}
