using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace AgendaTattoo.BLL.Services;

public class PublicBookingService : IPublicBookingService
{
    private readonly AgendaTattooDbContext _db;
    private readonly IAvailabilityService _availabilityService;

    public PublicBookingService(AgendaTattooDbContext db, IAvailabilityService availabilityService)
    {
        _db = db;
        _availabilityService = availabilityService;
    }

    public async Task<PublicStudioDto> GetStudioAsync(string slug, CancellationToken ct = default)
    {
        var studio = await FindStudioAsync(slug, ct);

        return new PublicStudioDto
        {
            Name = studio.Name,
            Description = studio.Description,
            Address = studio.Address,
            PhoneNumber = studio.PhoneNumber,
        };
    }

    public async Task<IReadOnlyList<PublicServiceDto>> ListServicesAsync(string slug, CancellationToken ct = default)
    {
        var studio = await FindStudioAsync(slug, ct);

        var services = await _db.Services
            .Where(s => s.StudioId == studio.Id && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

        return services.Select(s => new PublicServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
        }).ToList();
    }

    public async Task<IReadOnlyList<PublicArtistDto>> ListArtistsAsync(string slug, CancellationToken ct = default)
    {
        var studio = await FindStudioAsync(slug, ct);

        var artists = await _db.Users
            .Where(u => u.StudioId == studio.Id && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        return artists.Select(a => new PublicArtistDto { Id = a.Id, FullName = a.FullName }).ToList();
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        string slug, Guid artistId, Guid serviceId, DateOnly date, CancellationToken ct = default)
    {
        var studio = await FindStudioAsync(slug, ct);

        if (date < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ValidationAppException("Não é possível consultar disponibilidade em uma data passada.");

        return await _availabilityService.GetAvailableSlotsAsync(studio.Id, artistId, serviceId, date, ct);
    }

    public async Task<PublicBookingResponse> CreateBookingAsync(
        string slug, CreatePublicBookingRequest request, CancellationToken ct = default)
    {
        var studio = await FindStudioAsync(slug, ct);

        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.StudioId == studio.Id && s.IsActive, ct)
            ?? throw new NotFoundException("Serviço não encontrado.");

        var artistExists = await _db.Users.AnyAsync(u => u.Id == request.ArtistId && u.StudioId == studio.Id && u.IsActive, ct);
        if (!artistExists)
            throw new NotFoundException("Artista não encontrado.");

        var endsAt = request.StartsAt.AddMinutes(service.DurationMinutes);

        // 1) Confere que o horário pedido bate com um dos slots calculados (dentro do expediente do
        //    artista e sem colisão conhecida) — o cliente só pode escolher o que a própria API ofereceu.
        var date = DateOnly.FromDateTime(request.StartsAt.UtcDateTime);
        var offeredSlots = await _availabilityService.GetAvailableSlotsAsync(studio.Id, request.ArtistId, request.ServiceId, date, ct);

        if (!offeredSlots.Any(s => s.StartsAt == request.StartsAt))
            throw new ConflictException("Esse horário não está mais disponível. Escolha outro horário.");

        // 2) Reconfirma a ausência de colisão logo antes de gravar, dentro de uma transação, para reduzir
        //    ao mínimo a janela de corrida entre dois clientes tentando o mesmo horário ao mesmo tempo.
        //    Uma garantia 100% livre de corrida exigiria uma constraint de exclusão por intervalo no
        //    Postgres (EXCLUDE USING gist) — fica como possível reforço futuro.
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var hasOverlap = await _db.Appointments.AnyAsync(a =>
            a.ArtistId == request.ArtistId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartsAt < endsAt &&
            a.EndsAt > request.StartsAt, ct);

        if (hasOverlap)
            throw new ConflictException("Esse horário acabou de ser reservado por outra pessoa. Escolha outro horário.");

        // Encontra um cliente já cadastrado pelo telefone (dentro do mesmo estúdio) ou cria um novo —
        // clientes finais não têm conta, então é o telefone que os identifica entre visitas.
        var client = await _db.Clients.FirstOrDefaultAsync(
            c => c.StudioId == studio.Id && c.PhoneNumber == request.ClientPhoneNumber, ct);

        if (client is null)
        {
            client = new Client
            {
                Id = Guid.NewGuid(),
                StudioId = studio.Id,
                FullName = request.ClientFullName.Trim(),
                PhoneNumber = request.ClientPhoneNumber.Trim(),
                Email = request.ClientEmail?.Trim(),
            };
            _db.Clients.Add(client);
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            StudioId = studio.Id,
            ArtistId = request.ArtistId,
            ClientId = client.Id,
            ServiceId = request.ServiceId,
            StartsAt = request.StartsAt,
            EndsAt = endsAt,
            Status = AppointmentStatus.PendingConfirmation,
            Notes = request.Notes?.Trim(),
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new PublicBookingResponse
        {
            AppointmentId = appointment.Id,
            StartsAt = appointment.StartsAt,
            EndsAt = appointment.EndsAt,
            Status = appointment.Status.ToString(),
        };
    }

    private async Task<Studio> FindStudioAsync(string slug, CancellationToken ct)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        var studio = await _db.Studios.FirstOrDefaultAsync(s => s.Slug == normalized && s.IsActive, ct);
        return studio ?? throw new NotFoundException("Estúdio não encontrado.");
    }
}
