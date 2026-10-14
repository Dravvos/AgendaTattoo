using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Services
{
    /// <summary>
    /// Texto de consentimento atual, mostrado ao cliente antes do envio e gravado como cópia em cada
    /// ficha enviada. ATENÇÃO: isto é só um texto de partida para o MVP — revisar com um advogado
    /// antes de usar em produção, especialmente por envolver dado de saúde (LGPD, art. 11).
    /// </summary>
    public static class AnamnesisConsentText
    {
        public const string Current =
            "Declaro que as informações preenchidas nesta ficha são verdadeiras e completas. " +
            "Estou ciente dos riscos inerentes ao procedimento de tatuagem e autorizo sua realização " +
            "pelo profissional responsável. Entendo que devo informar ao estúdio qualquer alteração " +
            "no meu estado de saúde antes do início da sessão.";
    }
    public class AnamnesisService : IAnamnesisService
    {
        private readonly AgendaTattooDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public AnamnesisService(AgendaTattooDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<AnamnesisFormStatusDto> GetPublicStatusAsync(string slug, Guid appointmentId, CancellationToken ct = default)
        {
            var appointment = await FindPublicAppointmentAsync(slug, appointmentId, ct);

            var submitted = await _db.AnamnesisForms.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);

            return new AnamnesisFormStatusDto
            {
                Submitted = submitted is not null,
                SubmittedAt = submitted?.SubmittedAt,
                ClientFirstName = appointment.Client.FullName.Split(' ').FirstOrDefault(),
                ServiceName = appointment.Service.Name,
                AppointmentStartsAt = appointment.StartsAt,
                ConsentText = AnamnesisConsentText.Current,
            };
        }

        public async Task<AnamnesisFormStatusDto> SubmitAsync(
            string slug, Guid appointmentId, CreateAnamnesisFormRequest request, string submittedFromIp, CancellationToken ct = default)
        {
            if (!request.ConsentAccepted)
                throw new ValidationAppException("É preciso confirmar o consentimento para enviar a ficha.");

            var appointment = await FindPublicAppointmentAsync(slug, appointmentId, ct);

            var existing = await _db.AnamnesisForms.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);
            var form = existing ?? new AnamnesisForm { Id = Guid.NewGuid(), AppointmentId = appointmentId };

            form.StudioId = appointment.StudioId;
            form.ClientId = appointment.ClientId;
            form.HasAllergies = request.HasAllergies;
            form.AllergiesDescription = request.AllergiesDescription?.Trim();
            form.HasSkinConditions = request.HasSkinConditions;
            form.SkinConditionsDescription = request.SkinConditionsDescription?.Trim();
            form.HasDiabetes = request.HasDiabetes;
            form.HasBloodClottingDisorder = request.HasBloodClottingDisorder;
            form.HasHeartCondition = request.HasHeartCondition;
            form.HasEpilepsy = request.HasEpilepsy;
            form.IsPregnantOrBreastfeeding = request.IsPregnantOrBreastfeeding;
            form.UsesBloodThinners = request.UsesBloodThinners;
            form.UsesIsotretinoin = request.UsesIsotretinoin;
            form.HasKeloidHistory = request.HasKeloidHistory;
            form.ConsumedAlcoholOrDrugsLast24h = request.ConsumedAlcoholOrDrugsLast24h;
            form.HadPreviousTattooReaction = request.HadPreviousTattooReaction;
            form.PreviousTattooReactionDescription = request.PreviousTattooReactionDescription?.Trim();
            form.CurrentMedications = request.CurrentMedications?.Trim();
            form.AdditionalNotes = request.AdditionalNotes?.Trim();
            form.ConsentAccepted = request.ConsentAccepted;
            form.ConsentText = AnamnesisConsentText.Current;
            form.SignatureFullName = request.SignatureFullName.Trim();
            form.SubmittedAt = DateTimeOffset.UtcNow;
            form.SubmittedFromIp = submittedFromIp;

            if (existing is null)
                _db.AnamnesisForms.Add(form);

            await _db.SaveChangesAsync(ct);

            return await GetPublicStatusAsync(slug, appointmentId, ct);
        }

        public async Task<AnamnesisFormDto?> GetForStudioAsync(Guid appointmentId, CancellationToken ct = default)
        {
            var studioId = _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");
            var isOwner = _currentUser.Roles.Contains(Roles.Owner);

            var appointment = await _db.Appointments.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.StudioId == studioId, ct)
                ?? throw new NotFoundException("Agendamento não encontrado.");

            if (!isOwner && appointment.ArtistId != _currentUser.UserId)
                throw new ForbiddenException("Você só pode ver a ficha dos próprios agendamentos.");

            var form = await _db.AnamnesisForms.AsNoTracking()
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct);

            if (form is null)
                return null;

            return new AnamnesisFormDto
            {
                Id = form.Id,
                AppointmentId = form.AppointmentId,
                ClientId = form.ClientId,
                ClientName = form.Client.FullName,
                HasAllergies = form.HasAllergies,
                AllergiesDescription = form.AllergiesDescription,
                HasSkinConditions = form.HasSkinConditions,
                SkinConditionsDescription = form.SkinConditionsDescription,
                HasDiabetes = form.HasDiabetes,
                HasBloodClottingDisorder = form.HasBloodClottingDisorder,
                HasHeartCondition = form.HasHeartCondition,
                HasEpilepsy = form.HasEpilepsy,
                IsPregnantOrBreastfeeding = form.IsPregnantOrBreastfeeding,
                UsesBloodThinners = form.UsesBloodThinners,
                UsesIsotretinoin = form.UsesIsotretinoin,
                HasKeloidHistory = form.HasKeloidHistory,
                ConsumedAlcoholOrDrugsLast24h = form.ConsumedAlcoholOrDrugsLast24h,
                HadPreviousTattooReaction = form.HadPreviousTattooReaction,
                PreviousTattooReactionDescription = form.PreviousTattooReactionDescription,
                CurrentMedications = form.CurrentMedications,
                AdditionalNotes = form.AdditionalNotes,
                ConsentAccepted = form.ConsentAccepted,
                SignatureFullName = form.SignatureFullName,
                SubmittedAt = form.SubmittedAt,
            };
        }

        private async Task<Appointment> FindPublicAppointmentAsync(string slug, Guid appointmentId, CancellationToken ct)
        {
            var normalizedSlug = slug.Trim().ToLowerInvariant();

            var appointment = await _db.Appointments.AsNoTracking()
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Include(a => a.Studio)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Studio.Slug == normalizedSlug, ct);

            return appointment ?? throw new NotFoundException("Agendamento não encontrado.");
        }
    }
}
