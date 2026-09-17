using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }

        public Guid StudioId { get; set; }
        public Studio Studio { get; set; } = null!;

        public Guid ArtistId { get; set; }
        public ApplicationUser Artist { get; set; } = null!;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public DateTimeOffset StartsAt { get; set; }
        public DateTimeOffset EndsAt { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.PendingConfirmation;
        public string? Notes { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
