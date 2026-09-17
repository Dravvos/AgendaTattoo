using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Entities
{
    public class Studio
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Identificador público usado na URL de agendamento (ex.: /agendar/meu-estudio).
        /// Único no sistema. É por meio dele que o cliente final (sem login) acessa
        /// a agenda pública do estúdio.
        /// </summary>
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
