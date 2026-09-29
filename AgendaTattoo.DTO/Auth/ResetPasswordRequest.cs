using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Auth
{
    public class ResetPasswordRequest
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

    }
}
