using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgendaTattoo.DTO.Studio
{
    public class UpdateStaffMemberRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;
        
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}
