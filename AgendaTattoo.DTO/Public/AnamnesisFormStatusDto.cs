using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Public
{
    /// <summary>
    /// Retornado antes do envio (pra mostrar o resumo do agendamento + texto de consentimento) e
    /// depois do envio (pra confirmar que foi recebido). O frontend nunca precisa guardar o texto
    /// de consentimento por conta própria — ele sempre vem daqui, do backend.
    /// </summary>
    public class AnamnesisFormStatusDto
    {
        public bool Submitted { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public string? ClientFirstName { get; set; }
        public string? ServiceName { get; set; }
        public DateTimeOffset? AppointmentStartsAt { get; set; }
        public string ConsentText { get; set; } = string.Empty;
    }
}
