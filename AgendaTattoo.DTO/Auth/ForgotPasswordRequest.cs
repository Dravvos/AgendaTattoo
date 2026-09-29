using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Auth
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

    }
}
