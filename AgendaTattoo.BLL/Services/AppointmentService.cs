using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace AgendaTattoo.BLL.Services;

public class AppointmentService : IAppointmentService
{
    // Máquina de estados simples: de cada status, só se pode ir para os status listados.
    // Cancelled/Completed/NoShow são estados terminais.
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> AllowedTransitions = new()
    {
        [AppointmentStatus.PendingConfirmation] = [AppointmentStatus.Confirmed, AppointmentStatus.Cancelled],
        [AppointmentStatus.Confirmed] = [AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.NoShow] = [],
    };

    private readonly AgendaTattooDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AppointmentService(AgendaTattooDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");
    private bool IsOwner => _currentUser.Roles.Contains(Roles.Owner);

    public async Task<IReadOnlyList<AppointmentDto>> ListAsync(DateTimeOffset from, DateTimeOffset to, Guid? artistId, CancellationToken ct = default)
    {
        if (to <= from)
            throw new ValidationAppException("O período informado é inválido.");

        var effectiveArtistId = ResolveArtistFilter(artistId);

        var query = _db.Appointments
            .Include(a => a.Artist)
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Where(a => a.StudioId == StudioId && a.StartsAt < to && a.EndsAt > from);

        if (effectiveArtistId is not null)
            query = query.Where(a => a.ArtistId == effectiveArtistId);

        var appointments = await query.OrderBy(a => a.StartsAt).ToListAsync(ct);
        return appointments.Select(ToDto).ToList();
    }

    public async Task<AppointmentDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var appointment = await FindOwnedAsync(id, ct);
        return ToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken ct = default)
    {
        if (!IsOwner && _currentUser.UserId != request.ArtistId)
            throw new ForbiddenException("Você só pode criar agendamentos na sua própria agenda.");

        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.StudioId == StudioId && s.IsActive, ct)
            ?? throw new NotFoundException("Serviço não encontrado.");

        var artistExists = await _db.Users.AnyAsync(u => u.Id == request.ArtistId && u.StudioId == StudioId && u.IsActive, ct);
        if (!artistExists)
            throw new NotFoundException("Artista não encontrado.");

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == request.ClientId && c.StudioId == StudioId, ct);
        if (!clientExists)
            throw new NotFoundException("Cliente não encontrado.");

        var endsAt = request.StartsAt.AddMinutes(service.DurationMinutes);

        // Agendamento criado pela equipe (telefone, balcão etc.) não é obrigado a respeitar a grade de
        // horários de trabalho — só não pode colidir com outro agendamento já existente do artista.
        await EnsureNoOverlapAsync(request.ArtistId, request.StartsAt, endsAt, excludeAppointmentId: null, ct);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            StudioId = StudioId,
            ArtistId = request.ArtistId,
            ClientId = request.ClientId,
            ServiceId = request.ServiceId,
            StartsAt = request.StartsAt,
            EndsAt = endsAt,
            Status = AppointmentStatus.PendingConfirmation,
            Notes = request.Notes?.Trim(),
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync(ct);

        return await GetAsync(appointment.Id, ct);
    }

    public async Task<AppointmentDto> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken ct = default)
    {
        var appointment = await FindOwnedAsync(id, ct);
        EnsureCanManage(appointment);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.NoShow)
            throw new ValidationAppException("Não é possível reagendar um agendamento já finalizado ou cancelado.");

        var artistId = request.ArtistId ?? appointment.ArtistId;

        if (artistId != appointment.ArtistId)
        {
            if (!IsOwner)
                throw new ForbiddenException("Só o dono do estúdio pode transferir um agendamento para outro artista.");

            var artistExists = await _db.Users.AnyAsync(u => u.Id == artistId && u.StudioId == StudioId && u.IsActive, ct);
            if (!artistExists)
                throw new NotFoundException("Artista não encontrado.");
        }

        var service = await _db.Services.FirstAsync(s => s.Id == appointment.ServiceId, ct);
        var endsAt = request.StartsAt.AddMinutes(service.DurationMinutes);

        await EnsureNoOverlapAsync(artistId, request.StartsAt, endsAt, excludeAppointmentId: appointment.Id, ct);

        appointment.ArtistId = artistId;
        appointment.StartsAt = request.StartsAt;
        appointment.EndsAt = endsAt;

        await SaveWithConcurrencyCheckAsync(ct);

        return await GetAsync(appointment.Id, ct);
    }

    public async Task<AppointmentDto> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, CancellationToken ct = default)
    {
        var appointment = await FindOwnedAsync(id, ct);
        EnsureCanManage(appointment);

        var newStatus = (AppointmentStatus)request.Status;

        if (newStatus != appointment.Status && !AllowedTransitions[appointment.Status].Contains(newStatus))
            throw new ValidationAppException($"Não é possível mudar o status de '{appointment.Status}' para '{newStatus}'.");

        appointment.Status = newStatus;
        await SaveWithConcurrencyCheckAsync(ct);

        return await GetAsync(appointment.Id, ct);
    }

    private void EnsureCanManage(Appointment appointment)
    {
        if (!IsOwner && appointment.ArtistId != _currentUser.UserId)
            throw new ForbiddenException("Você só pode gerenciar os próprios agendamentos.");
    }

    private Guid? ResolveArtistFilter(Guid? requestedArtistId)
    {
        if (IsOwner)
            return requestedArtistId; // dono pode ver a agenda toda ou filtrar por um artista específico

        if (requestedArtistId is not null && requestedArtistId != _currentUser.UserId)
            throw new ForbiddenException("Você só pode ver a própria agenda.");

        return _currentUser.UserId; // artista comum só enxerga a própria agenda
    }

    private async Task EnsureNoOverlapAsync(Guid artistId, DateTimeOffset startsAt, DateTimeOffset endsAt, Guid? excludeAppointmentId, CancellationToken ct)
    {
        var hasOverlap = await _db.Appointments.AnyAsync(a =>
            a.ArtistId == artistId &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Id != (excludeAppointmentId ?? Guid.Empty) &&
            a.StartsAt < endsAt &&
            a.EndsAt > startsAt, ct);

        if (hasOverlap)
            throw new ConflictException("Já existe um agendamento para esse artista nesse horário.");
    }

    private async Task SaveWithConcurrencyCheckAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Este agendamento foi alterado por outra pessoa nesse meio tempo. Recarregue e tente novamente.");
        }
    }

    private async Task<Appointment> FindOwnedAsync(Guid id, CancellationToken ct)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Artist)
            .Include(a => a.Client)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id && a.StudioId == StudioId, ct);

        if (appointment is null)
            throw new NotFoundException("Agendamento não encontrado.");

        // Mesma mensagem de "não existe" quando o agendamento é de outro artista: não revela a existência
        // do registro para quem não tem permissão de vê-lo.
        if (!IsOwner && appointment.ArtistId != _currentUser.UserId)
            throw new NotFoundException("Agendamento não encontrado.");

        return appointment;
    }

    private static AppointmentDto ToDto(Appointment a) => new()
    {
        Id = a.Id,
        ArtistId = a.ArtistId,
        ArtistName = a.Artist.FullName,
        ClientId = a.ClientId,
        ClientName = a.Client.FullName,
        ServiceId = a.ServiceId,
        ServiceName = a.Service.Name,
        StartsAt = a.StartsAt,
        EndsAt = a.EndsAt,
        Status = (AppointmentStatusDto)a.Status,
        Notes = a.Notes,
    };
}
