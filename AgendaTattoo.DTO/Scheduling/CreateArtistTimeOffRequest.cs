using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Scheduling
{
    public class CreateArtistTimeOffRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        /// <summary>Deixe em branco junto com EndTime para bloquear o dia inteiro (ex.: feriado).</summary>
        public TimeOnly? StartTime { get; set; }

        /// <summary>Deixe em branco junto com StartTime para bloquear o dia inteiro.</summary>
        public TimeOnly? EndTime { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }

    }
}
