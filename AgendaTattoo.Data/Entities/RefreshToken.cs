using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string TokenHash { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>Hash do token que substituiu este (rotação de refresh token).</summary>
        public string? ReplacedByTokenHash { get; set; }

        public string CreatedByIp { get; set; } = string.Empty;

        public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
    }
}
