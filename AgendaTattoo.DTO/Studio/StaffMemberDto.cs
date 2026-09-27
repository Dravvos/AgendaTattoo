using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.DTO.Studio
{
    public class StaffMemberDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>"Owner" ou "Artist" — espelha AgendaTattoo.Data.Entities.Roles.</summary>
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }

    }
}
