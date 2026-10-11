using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public class ArtistTimeOff
    {
        public Guid Id { get; set; }

        public Guid ArtistId { get; set; }
        public ApplicationUser Artist { get; set; } = null!;

        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public string? Reason { get; set; }

    }
}
