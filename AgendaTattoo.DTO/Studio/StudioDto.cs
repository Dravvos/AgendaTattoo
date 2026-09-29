using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Studio
{
    public class StudioDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>Identificador público usado na URL de agendamento (ex.: /agendar/meu-estudio).</summary>
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        /// <summary>IANA timezone id (ex.: "America/Sao_Paulo").</summary>
        public string TimeZoneId { get; set; } = string.Empty;

    }
}
