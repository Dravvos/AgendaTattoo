using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Auth
{
    public class RegisterRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string StudioName { get; set; } = string.Empty;

        /// <summary>Identificador público do estúdio (usado na URL de agendamento). Ex.: "tinta-negra-tattoo".</summary>
        [Required, StringLength(80, MinimumLength = 2)]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Use apenas letras minúsculas, números e hífen.")]
        public string StudioSlug { get; set; } = string.Empty;

        [Required, StringLength(150, MinimumLength = 2)]
        public string OwnerFullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
