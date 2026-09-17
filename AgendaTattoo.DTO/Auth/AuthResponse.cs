using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
