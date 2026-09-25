using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace AgendaTattoo.BLL.Services;

public class AvailabilityService : IAvailabilityService
{
    // Granularidade dos horários oferecidos ao cliente (ex.: 10:00, 10:15, 10:30...).
    private const int SlotGranularityMinutes = 15;

    private readonly AgendaTattooDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AvailabilityService(AgendaTattooDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");

    public async Task<IReadOnlyList<WorkingHoursDto>> GetWorkingHoursAsync(Guid artistId, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(artistId, ct);

        var hours = await _db.ArtistWorkingHours
            .Where(w => w.ArtistId == artistId)
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync(ct);

        return hours.Select(w => new WorkingHoursDto
        {
            DayOfWeek = w.DayOfWeek,
            StartTime = w.StartTime,
            EndTime = w.EndTime,
        }).ToList();
    }

    public async Task SetWorkingHoursAsync(Guid artistId, SetWorkingHoursRequest request, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(artistId, ct);

        foreach (var entry in request.Days)
        {
            if (entry.EndTime <= entry.StartTime)
                throw new ValidationAppException($"Horário inválido para {entry.DayOfWeek}: o término deve ser depois do início.");
        }

        var duplicatedDays = request.Days
            .GroupBy(d => d.DayOfWeek)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatedDays.Count > 0)
            throw new ValidationAppException("Cada dia da semana só pode ter uma faixa de horário.");

        var existing = await _db.ArtistWorkingHours.Where(w => w.ArtistId == artistId).ToListAsync(ct);
        _db.ArtistWorkingHours.RemoveRange(existing);

        foreach (var entry in request.Days)
        {
            _db.ArtistWorkingHours.Add(new ArtistWorkingHours
            {
                Id = Guid.NewGuid(),
                ArtistId = artistId,
                DayOfWeek = entry.DayOfWeek,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid studioId, Guid artistId, Guid serviceId, DateOnly date, CancellationToken ct = default)
    {
        var studio = await _db.Studios.FirstOrDefaultAsync(s => s.Id == studioId, ct)
            ?? throw new NotFoundException("Estúdio não encontrado.");

        var service = await _db.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.StudioId == studioId && s.IsActive, ct)
            ?? throw new NotFoundException("Serviço não encontrado.");

        var artistExists = await _db.Users.AnyAsync(u => u.Id == artistId && u.StudioId == studioId && u.IsActive, ct);
        if (!artistExists)
            throw new NotFoundException("Artista não encontrado.");

        var workingHours = await _db.ArtistWorkingHours
            .FirstOrDefaultAsync(w => w.ArtistId == artistId && w.DayOfWeek == date.DayOfWeek, ct);

        if (workingHours is null)
            return []; // artista não trabalha nesse dia da semana

        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(studio.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ValidationAppException("Fuso horário do estúdio configurado incorretamente.");
        }

        var dayStartLocal = date.ToDateTime(workingHours.StartTime, DateTimeKind.Unspecified);
        var dayEndLocal = date.ToDateTime(workingHours.EndTime, DateTimeKind.Unspecified);

        var dayStartUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, timeZone), TimeSpan.Zero);
        var dayEndUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, timeZone), TimeSpan.Zero);

        var duration = TimeSpan.FromMinutes(service.DurationMinutes);
        var step = TimeSpan.FromMinutes(SlotGranularityMinutes);

        // Só os agendamentos ativos daquele artista que se sobrepõem à janela do dia importam para o cálculo.
        var dayAppointments = await _db.Appointments
            .Where(a => a.ArtistId == artistId
                && a.Status != AppointmentStatus.Cancelled
                && a.StartsAt < dayEndUtc
                && a.EndsAt > dayStartUtc)
            .Select(a => new { a.StartsAt, a.EndsAt })
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var slots = new List<AvailableSlotDto>();

        for (var slotStart = dayStartUtc; slotStart + duration <= dayEndUtc; slotStart += step)
        {
            if (slotStart < now)
                continue;

            var slotEnd = slotStart + duration;
            var overlaps = dayAppointments.Any(a => slotStart < a.EndsAt && slotEnd > a.StartsAt);
            if (overlaps)
                continue;

            slots.Add(new AvailableSlotDto { StartsAt = slotStart, EndsAt = slotEnd });
        }

        return slots;
    }

    private async Task EnsureCanManageAsync(Guid artistId, CancellationToken ct)
    {
        var isOwner = _currentUser.Roles.Contains(Roles.Owner);
        var isSelf = _currentUser.UserId == artistId;

        if (!isOwner && !isSelf)
            throw new ForbiddenException("Você só pode gerenciar os próprios horários de trabalho.");

        var artistExists = await _db.Users.AnyAsync(u => u.Id == artistId && u.StudioId == StudioId, ct);
        if (!artistExists)
            throw new NotFoundException("Artista não encontrado.");
    }
}
