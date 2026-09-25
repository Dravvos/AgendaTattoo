using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace AgendaTattoo.BLL.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly AgendaTattooDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ServiceCatalogService(AgendaTattooDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");

    public async Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken ct = default)
    {
        var services = await _db.Services
            .Where(s => s.StudioId == StudioId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return services.Select(ToDto).ToList();
    }

    public async Task<ServiceDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var service = await FindOwnedAsync(id, ct);
        return ToDto(service);
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceRequest request, CancellationToken ct = default)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            StudioId = StudioId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = true,
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync(ct);

        return ToDto(service);
    }

    public async Task<ServiceDto> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken ct = default)
    {
        var service = await FindOwnedAsync(id, ct);

        service.Name = request.Name.Trim();
        service.Description = request.Description?.Trim();
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;
        service.IsActive = request.IsActive;

        await _db.SaveChangesAsync(ct);

        return ToDto(service);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var service = await FindOwnedAsync(id, ct);
        service.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<Service> FindOwnedAsync(Guid id, CancellationToken ct)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == id && s.StudioId == StudioId, ct);

        // Mensagem genérica de propósito: não revela se o serviço existe em outro estúdio.
        return service ?? throw new NotFoundException("Serviço não encontrado.");
    }

    private static ServiceDto ToDto(Service s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        DurationMinutes = s.DurationMinutes,
        Price = s.Price,
        IsActive = s.IsActive,
    };
}
