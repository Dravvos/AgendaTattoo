using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Interfaces
{
    public interface IAnamnesisService
    {
        /// <summary>Visão pública (sem login): resumo do agendamento, texto de consentimento atual e se a ficha já foi enviada.</summary>
        Task<AnamnesisFormStatusDto> GetPublicStatusAsync(string slug, Guid appointmentId, CancellationToken ct = default);

        /// <summary>Envio pelo próprio cliente, sem login. Reenviar substitui a ficha anterior do mesmo agendamento.</summary>
        Task<AnamnesisFormStatusDto> SubmitAsync(string slug, Guid appointmentId, CreateAnamnesisFormRequest request, string submittedFromIp, CancellationToken ct = default);

        /// <summary>Visão interna para a equipe do estúdio (dono, ou o próprio artista do agendamento). Retorna null se o cliente ainda não enviou.</summary>
        Task<AnamnesisFormDto?> GetForStudioAsync(Guid appointmentId, CancellationToken ct = default);
    }
}
