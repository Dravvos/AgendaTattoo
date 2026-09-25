using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace AgendaTattoo.BLL.Services;

public class ClientService : IClientService
{
    private readonly AgendaTattooDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ClientService(AgendaTattooDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");

    public async Task<IReadOnlyList<ClientDto>> ListAsync(string? search, CancellationToken ct = default)
    {
        var query = _db.Clients.Where(c => c.StudioId == StudioId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => EF.Functions.ILike(c.FullName, $"%{term}%") || EF.Functions.ILike(c.PhoneNumber, $"%{term}%"));
        }

        var clients = await query.OrderBy(c => c.FullName).ToListAsync(ct);
        return clients.Select(ToDto).ToList();
    }

    public async Task<ClientDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var client = await FindOwnedAsync(id, ct);
        return ToDto(client);
    }

    public async Task<ClientDto> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            StudioId = StudioId,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = request.Email?.Trim(),
            Notes = request.Notes?.Trim(),
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync(ct);

        return ToDto(client);
    }

    public async Task<ClientDto> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await FindOwnedAsync(id, ct);

        client.FullName = request.FullName.Trim();
        client.PhoneNumber = request.PhoneNumber.Trim();
        client.Email = request.Email?.Trim();
        client.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync(ct);

        return ToDto(client);
    }

    private async Task<Client> FindOwnedAsync(Guid id, CancellationToken ct)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id && c.StudioId == StudioId, ct);
        return client ?? throw new NotFoundException("Cliente não encontrado.");
    }

    private static ClientDto ToDto(Client c) => new()
    {
        Id = c.Id,
        FullName = c.FullName,
        PhoneNumber = c.PhoneNumber,
        Email = c.Email,
        Notes = c.Notes,
    };
}
