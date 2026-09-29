using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Studio
{
    public class UpdateStudioRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Trocar o slug muda a URL pública de agendamento — qualquer link já compartilhado
        /// com clientes para de funcionar. O frontend deve avisar isso antes de salvar.
        /// </summary>
        [Required, StringLength(80, MinimumLength = 2)]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Use apenas letras minúsculas, números e hífen.")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Phone, StringLength(30)]
        public string? PhoneNumber { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Required]
        public string TimeZoneId { get; set; } = string.Empty;

    }
}
