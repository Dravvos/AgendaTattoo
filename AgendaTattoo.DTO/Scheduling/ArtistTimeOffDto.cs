using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Scheduling
{
    public class ArtistTimeOffDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }

        /// <summary>Nulos junto com EndTime = dia inteiro bloqueado. Preenchidos = só esse intervalo.</summary>
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? Reason { get; set; }

    }
}
