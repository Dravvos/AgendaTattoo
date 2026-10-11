using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgendaTattoo.BLL.Services;

public class AvailabilityService : IAvailabilityService
{
    // Granularidade dos horários oferecidos ao cliente (ex.: 10:00, 10:15, 10:30...).
    private const int SlotGranularityMinutes = 15;

    // Limite do período aceito por GetAvailabilityCalendarAsync (endpoint público, sem login).
    private const int MaxCalendarRangeDays = 366;

    private readonly AgendaTattooDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AvailabilityService> _logger;

    public AvailabilityService(AgendaTattooDbContext db, ICurrentUserService currentUser, ILogger<AvailabilityService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");

    public async Task<IReadOnlyList<WorkingHoursDto>> GetWorkingHoursAsync(Guid artistId, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(artistId, ct);

        var hours = await _db.ArtistWorkingHours
            .Where(w => w.ArtistId == artistId)
            .OrderBy(w => w.DayOfWeek)
            .ThenBy(w => w.StartTime)
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

        // Cada dia pode ter mais de uma faixa (ex.: manhã e tarde, com intervalo de almoço no
        // meio), mas as faixas do mesmo dia não podem se sobrepor entre si.
        var overlappingDays = request.Days
            .GroupBy(d => d.DayOfWeek)
            .Where(group => HasOverlap(group.Select(d => (d.StartTime, d.EndTime))))
            .Select(group => group.Key)
            .ToList();

        if (overlappingDays.Count > 0)
            throw new ValidationAppException("Duas faixas de horário do mesmo dia não podem se sobrepor.");

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

        var workingBlocks = await _db.ArtistWorkingHours
            .Where(w => w.ArtistId == artistId && w.DayOfWeek == date.DayOfWeek)
            .OrderBy(w => w.StartTime)
            .ToListAsync(ct);

        if (workingBlocks.Count == 0)
            return []; // artista não trabalha nesse dia da semana

        var timeOffs = await _db.ArtistTimeOffs
            .Where(t => t.ArtistId == artistId && t.Date == date)
            .ToListAsync(ct);

        // Folga do dia inteiro (feriado, dia de folga): nem vale a pena calcular o resto.
        if (timeOffs.Any(t => t.StartTime is null && t.EndTime is null))
        {
            _logger.LogInformation(
                "Disponibilidade {Date} artista {ArtistId}: folga de dia inteiro, nenhum horário oferecido.", date, artistId);
            return [];
        }

        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(studio.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ValidationAppException("Fuso horário do estúdio configurado incorretamente.");
        }

        DateTimeOffset ToUtc(TimeOnly time)
        {
            var local = date.ToDateTime(time, DateTimeKind.Unspecified);
            return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, timeZone), TimeSpan.Zero);
        }

        // Cada faixa de trabalho (ex.: manhã e tarde, com intervalo de almoço no meio) vira uma
        // janela UTC separada; folgas parciais do dia recortam essas janelas mais abaixo.
        var workWindows = workingBlocks
            .Select(w => (Start: ToUtc(w.StartTime), End: ToUtc(w.EndTime)))
            .ToList();

        var partialTimeOffWindows = timeOffs
            .Where(t => t.StartTime is not null && t.EndTime is not null)
            .Select(t => (Start: ToUtc(t.StartTime!.Value), End: ToUtc(t.EndTime!.Value)))
            .ToList();

        var dayStartUtc = workWindows.Min(w => w.Start);
        var dayEndUtc = workWindows.Max(w => w.End);

        // Só os agendamentos ativos daquele artista que se sobrepõem à janela do dia importam para o cálculo.
        var dayAppointments = await _db.Appointments
            .Where(a => a.ArtistId == artistId
                && a.Status != AppointmentStatus.Cancelled
                && a.StartsAt < dayEndUtc
                && a.EndsAt > dayStartUtc)
            .Select(a => new { a.StartsAt, a.EndsAt })
            .ToListAsync(ct);

        var duration = TimeSpan.FromMinutes(service.DurationMinutes);
        var step = TimeSpan.FromMinutes(SlotGranularityMinutes);
        var now = DateTimeOffset.UtcNow;
        var slots = new List<AvailableSlotDto>();

        foreach (var window in workWindows)
        {
            for (var slotStart = window.Start; slotStart + duration <= window.End; slotStart += step)
            {
                if (slotStart < now)
                    continue;

                var slotEnd = slotStart + duration;

                var overlapsAppointment = dayAppointments.Any(a => slotStart < a.EndsAt && slotEnd > a.StartsAt);
                if (overlapsAppointment)
                    continue;

                var overlapsTimeOff = partialTimeOffWindows.Any(t => slotStart < t.End && slotEnd > t.Start);
                if (overlapsTimeOff)
                    continue;

                slots.Add(new AvailableSlotDto { StartsAt = slotStart, EndsAt = slotEnd });
            }
        }

        _logger.LogInformation(
            "Disponibilidade {Date} artista {ArtistId} (fuso {TimeZone}): {Windows} faixa(s) de trabalho, " +
            "{PartialTimeOffs} folga(s) parcial(is), {Appointments} agendamento(s), {Slots} horário(s) livre(s).",
            date, artistId, studio.TimeZoneId, workWindows.Count, partialTimeOffWindows.Count, dayAppointments.Count, slots.Count);

        return slots.OrderBy(s => s.StartsAt).ToList();
    }

    public async Task<PublicAvailabilityCalendarDto> GetAvailabilityCalendarAsync(
    Guid studioId, Guid artistId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (to < from || to.DayNumber - from.DayNumber > MaxCalendarRangeDays)
            throw new ValidationAppException("O período informado é inválido.");

        var artistExists = await _db.Users.AnyAsync(u => u.Id == artistId && u.StudioId == studioId && u.IsActive, ct);
        if (!artistExists)
            throw new NotFoundException("Artista não encontrado.");

        var workingDays = await _db.ArtistWorkingHours
            .Where(w => w.ArtistId == artistId)
            .Select(w => w.DayOfWeek)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(ct);

        // Só folgas de dia inteiro (sem horário) bloqueiam a data no calendário; folgas parciais
        // só retiram horários de dentro do dia, e isso já é tratado em GetAvailableSlotsAsync.
        var blockedDates = await _db.ArtistTimeOffs
            .Where(t => t.ArtistId == artistId
                && t.Date >= from && t.Date <= to
                && t.StartTime == null && t.EndTime == null)
            .Select(t => t.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync(ct);

        _logger.LogInformation(
            "Calendário artista {ArtistId} {From}..{To}: {WorkingDays} dia(s) da semana atendidos, {Blocked} data(s) bloqueada(s).",
            artistId, from, to, workingDays.Count, blockedDates.Count);

        return new PublicAvailabilityCalendarDto
        {
            WorkingDays = workingDays,
            BlockedDates = blockedDates,
        };
    }

    public async Task<IReadOnlyList<ArtistTimeOffDto>> ListTimeOffAsync(
        Guid artistId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(artistId, ct);

        if (to < from)
            throw new ValidationAppException("O período informado é inválido.");

        var timeOffs = await _db.ArtistTimeOffs
            .Where(t => t.ArtistId == artistId && t.Date >= from && t.Date <= to)
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

        return timeOffs.Select(ToDto).ToList();
    }

    public async Task<ArtistTimeOffDto> AddTimeOffAsync(Guid artistId, CreateArtistTimeOffRequest request, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(artistId, ct);

        if ((request.StartTime is null) != (request.EndTime is null))
            throw new ValidationAppException(
                "Informe início e término da folga parcial, ou deixe os dois em branco para bloquear o dia inteiro.");

        if (request.StartTime is not null && request.EndTime is not null && request.EndTime <= request.StartTime)
            throw new ValidationAppException("O término da folga deve ser depois do início.");

        var artistExists = await _db.Users.AnyAsync(u => u.Id == artistId && u.StudioId == StudioId, ct);
        if (!artistExists)
            throw new NotFoundException("Artista não encontrado.");

        var timeOff = new ArtistTimeOff
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Reason = request.Reason?.Trim(),
        };

        _db.ArtistTimeOffs.Add(timeOff);
        await _db.SaveChangesAsync(ct);

        return ToDto(timeOff);
    }

    public async Task RemoveTimeOffAsync(Guid artistId, Guid timeOffId, CancellationToken ct = default)
    {
        await EnsureCanManageAsync(artistId, ct);

        var timeOff = await _db.ArtistTimeOffs.FirstOrDefaultAsync(t => t.Id == timeOffId && t.ArtistId == artistId, ct)
            ?? throw new NotFoundException("Folga não encontrada.");

        _db.ArtistTimeOffs.Remove(timeOff);
        await _db.SaveChangesAsync(ct);
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

    /// <summary>true se algum par de faixas (ordenadas por início) se sobrepuser.</summary>
    private static bool HasOverlap(IEnumerable<(TimeOnly StartTime, TimeOnly EndTime)> ranges)
    {
        var ordered = ranges.OrderBy(r => r.StartTime).ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].StartTime < ordered[i - 1].EndTime)
                return true;
        }

        return false;
    }

    private static ArtistTimeOffDto ToDto(ArtistTimeOff t) => new()
    {
        Id = t.Id,
        Date = t.Date,
        StartTime = t.StartTime,
        EndTime = t.EndTime,
        Reason = t.Reason,
    };


}
