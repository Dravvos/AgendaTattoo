using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace AgendaTattoo.Data.Entities
{
    public class ApplicationUser:IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        /// <summary>Estúdio ao qual este usuário pertence (todo usuário pertence a exatamente um estúdio).</summary>
        public Guid StudioId { get; set; }
        public Studio Studio { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<ArtistWorkingHours> WorkingHours { get; set; } = new List<ArtistWorkingHours>();
    }
}
